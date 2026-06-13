# IIS HTTPS API Reverse Proxy Deployment Notes

## 背景

生产环境使用 IIS 部署前端 Vue 静态站点，访问地址为：

```text
https://oa.ygxinjian.com/
```

后端 API 也部署在同一台服务器 IIS 上，内网地址为：

```text
http://192.168.70.3:8077
```

登录页最初请求：

```text
POST http://192.168.70.3:8077/api/admin/user/login
```

浏览器控制台报错：

```text
Mixed Content
POST http://192.168.70.3:8077/api/admin/user/login net::ERR_EMPTY_RESPONSE
```

原因是 HTTPS 页面中直接请求了 HTTP API。浏览器会拦截这种混合内容请求；同时 `192.168.70.3` 是内网地址，外部客户端也不应直接访问该地址。

## 修正目标

浏览器只访问 HTTPS 同源地址：

```text
POST https://oa.ygxinjian.com/api/admin/user/login
```

再由 IIS 在服务器内部反向代理到后端：

```text
http://192.168.70.3:8077/api/admin/user/login
```

请求链路为：

```text
Browser
  -> https://oa.ygxinjian.com/api/admin/user/login
  -> IIS URL Rewrite + ARR
  -> http://192.168.70.3:8077/api/admin/user/login
```

## 前端配置

生产环境实际读取的是部署目录中的 `_app.config.js`：

```js
window._VBEN_ADMIN_PRO_APP_CONF_ = {
  VITE_GLOB_API_URL: '/api/admin',
};
```

也建议同步修改源码中的生产环境配置，避免重新打包后又恢复成内网 HTTP 地址：

```env
VITE_GLOB_API_URL=/api/admin
```

源码位置：

```text
src/frontend/apps/admin-antd/.env.production
```

注意：Vite 构建会生成 `_app.config.js`。已经部署的站点可以直接修改部署目录中的 `_app.config.js` 快速修复；后续重新打包时，应保证 `.env.production` 中的配置也是正确的。

## IIS 必需组件

IIS 需要安装并启用：

- URL Rewrite
- Application Request Routing, ARR

安装 ARR 后，需要在 IIS 管理器中启用代理：

```text
服务器节点
  -> Application Request Routing Cache
  -> Server Proxy Settings
  -> Enable proxy
  -> Apply
```

## 完整 web.config

Vue 站点根目录的 `web.config` 建议使用下面的完整配置。

规则顺序很重要：

1. SignalR 通知 Hub：`/api/admin/notification/*` 转到后端 `/notification/*`
2. SignalR 聊天 Hub：`/api/admin/chat/*` 转到后端 `/chat/*`
3. 普通 API：`/api/*` 转到后端 `/api/*`
4. Vue 前端路由兜底：最后才转到 `/index.html`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <!-- SignalR 通知 Hub：前端 /api/admin/notification -> 后端 /notification -->
        <rule name="ReverseProxyToSignalRNotification" stopProcessing="true">
          <match url="^api/admin/notification(.*)" />
          <action type="Rewrite" url="http://192.168.70.3:8077/notification{R:1}" />
          <serverVariables>
            <set name="HTTP_X_FORWARDED_PROTO" value="https" />
            <set name="HTTP_X_FORWARDED_HOST" value="{HTTP_HOST}" />
          </serverVariables>
        </rule>

        <!-- SignalR 聊天 Hub：前端 /api/admin/chat -> 后端 /chat -->
        <rule name="ReverseProxyToSignalRChat" stopProcessing="true">
          <match url="^api/admin/chat(.*)" />
          <action type="Rewrite" url="http://192.168.70.3:8077/chat{R:1}" />
          <serverVariables>
            <set name="HTTP_X_FORWARDED_PROTO" value="https" />
            <set name="HTTP_X_FORWARDED_HOST" value="{HTTP_HOST}" />
          </serverVariables>
        </rule>

        <!-- 普通 API：前端 /api/* -> 后端 /api/* -->
        <rule name="ReverseProxyToBackendApi" stopProcessing="true">
          <match url="^api/(.*)" />
          <action type="Rewrite" url="http://192.168.70.3:8077/api/{R:1}" />
          <serverVariables>
            <set name="HTTP_X_FORWARDED_PROTO" value="https" />
            <set name="HTTP_X_FORWARDED_HOST" value="{HTTP_HOST}" />
          </serverVariables>
        </rule>

        <!-- Vue hash/history 前端路由兜底，必须放在所有后端反代规则之后 -->
        <rule name="VueFallback" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

说明：

- URL Rewrite 的 `<match url="...">` 匹配的是不带开头 `/` 的路径，所以写 `^api/(.*)`，不是 `^/api/(.*)`。
- QueryString 默认会自动带到反代后的地址。例如 `/api/admin/notification/negotiate?negotiateVersion=1` 会转到 `/notification/negotiate?negotiateVersion=1`。
- SignalR 后端实际 Hub 路径是 `/notification` 和 `/chat`，不是 `/api/admin/notification` 和 `/api/admin/chat`，所以需要放在普通 API 规则之前单独处理。

## SignalR / WebSocket

后端 SignalR Hub 实际挂载路径：

```text
/notification
/chat
```

前端生产配置为 `VITE_GLOB_API_URL=/api/admin` 时，SignalR 会尝试连接：

```text
https://oa.ygxinjian.com/api/admin/notification
https://oa.ygxinjian.com/api/admin/chat
```

因此 IIS 必须使用上面的 SignalR 专用 URL Rewrite 规则，把它们转发到后端真实 Hub 路径。

同时确认 IIS 已启用 WebSocket：

```text
Windows 功能
  -> Internet Information Services
  -> World Wide Web Services
  -> Application Development Features
  -> WebSocket Protocol
```

如果未启用 WebSocket，SignalR 的 `/negotiate` 可能成功，但 WebSocket 建连阶段可能出现 `ERR_CONNECTION_CLOSED` 或自动降级失败。

## Server Variables

本次 IIS 中已添加并允许以下 URL Rewrite Server Variables：

```text
HTTP_X_FORWARDED_PROTO
HTTP_X_FORWARDED_HOST
```

添加位置：

```text
IIS 管理器
  -> 选中站点
  -> URL Rewrite
  -> View Server Variables
  -> Add
```

如果 `web.config` 配置了 `<serverVariables>`，但 IIS 未允许对应变量，请求会报：

```text
500 (URL Rewrite Module Error.)
```

遇到该错误时，优先检查 Server Variables 是否已添加并允许。

## 验证

修正后登录请求应变为：

```text
POST https://oa.ygxinjian.com/api/admin/user/login
```

不应再出现：

```text
POST http://192.168.70.3:8077/api/admin/user/login
Mixed Content
```

如果仍然请求内网 HTTP 地址，检查部署目录中的 `_app.config.js` 是否仍为：

```text
http://192.168.70.3:8077/api/admin
```

如果请求地址已经是 HTTPS 域名但返回 `URL Rewrite Module Error`，检查：

- ARR 是否安装并启用 `Enable proxy`
- `HTTP_X_FORWARDED_PROTO` 是否已添加到允许列表
- `HTTP_X_FORWARDED_HOST` 是否已添加到允许列表
- API 反代规则是否放在 Vue fallback 规则之前

如果 SignalR 报：

```text
/api/admin/notification/negotiate 404 Not Found
Either this is not a SignalR endpoint or there is a proxy blocking the connection.
```

检查：

- `ReverseProxyToSignalRNotification` 是否存在
- `ReverseProxyToSignalRChat` 是否存在
- 两条 SignalR 规则是否放在 `ReverseProxyToBackendApi` 之前
- IIS 是否启用 WebSocket Protocol
