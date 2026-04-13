#!/usr/bin/env python3
import os
import textwrap
from datetime import datetime


class CoachSession:
    def __init__(self):
        self.modules_done = []
        self.notes = []
        self.score = 0
        self.questions = 0

    def add_note(self, title: str, content: str):
        self.notes.append((title, content))

    def add_module(self, name: str, module_score: int, module_questions: int):
        self.modules_done.append((name, module_score, module_questions))
        self.score += module_score
        self.questions += module_questions


def prompt(msg: str) -> str:
    try:
        return input(msg).strip()
    except EOFError:
        return ""


def print_title(title: str):
    print("\n" + "=" * len(title))
    print(title)
    print("=" * len(title))


def wrap(s: str) -> str:
    return textwrap.fill(s, width=88)


def module_overview(session: CoachSession):
    print_title("模块：CleanDDD 总览与心智模型")
    print(wrap("CleanDDD 强调以领域模型为中心，围绕聚合、不变式、领域事件来组织代码。"))
    print(wrap("目标是通过清晰边界与事件驱动，降低耦合，提高可演进性与可测试性。"))
    ans = prompt("小测：CleanDDD 中跨聚合的影响应通过什么机制传递？ ")
    correct = "事件" in ans or "领域事件" in ans or "event" in ans.lower()
    print("参考答案：通过领域事件驱动跨聚合影响。")
    session.add_note("总览与心智模型", f"小测回答：{ans}；正确：{'是' if correct else '否'}")
    session.add_module("总览与心智模型", 1 if correct else 0, 1)


def module_aggregate(session: CoachSession):
    print_title("模块：聚合与不变式")
    print(wrap("聚合是事务一致性边界，聚合内部维护不变式，外界不能直接引用其实体。"))
    print("检查清单：\n- 边界是否明确\n- 不变式是否枚举\n- 是否避免共享实体\n- 允许共享值对象")
    ans = prompt("小测：是否允许跨聚合直接引用实体？(是/否) ")
    correct = ans in ("否", "no", "No", "NO")
    print("参考答案：否。跨聚合不能直接或间接引用实体。")
    session.add_note("聚合与不变式", f"小测回答：{ans}；正确：{'是' if correct else '否'}")
    session.add_module("聚合与不变式", 1 if correct else 0, 1)


def module_cqrs(session: CoachSession):
    print_title("模块：命令与查询 (CQRS)")
    print(wrap("命令用于改变状态（写），查询用于读取信息（读），命名统一用 PascalCase。"))
    ans = prompt("小测：创建订单应建模为命令还是查询？ ")
    correct = "命令" in ans or "command" in ans.lower()
    print("参考答案：命令。创建/修改/关闭等写操作映射为命令。")
    session.add_note("命令与查询", f"小测回答：{ans}；正确：{'是' if correct else '否'}")
    session.add_module("命令与查询", 1 if correct else 0, 1)


def module_events(session: CoachSession):
    print_title("模块：领域事件与处理器")
    print(wrap("领域事件由聚合行为产生，订阅方在事件处理器中执行反应式动作，避免跨聚合直接操作。"))
    ans = prompt("小测：订单已支付应对应怎样的事件命名？（建议过去式） ")
    correct = ("已支付" in ans) or ("Paid" in ans) or ("支付完成" in ans) or ("OrderPaid" in ans)
    print("参考答案：例如 OrderPaid。领域事件命名用过去式。")
    session.add_note("领域事件与处理器", f"小测回答：{ans}；正确：{'是' if correct else '否'}")
    session.add_module("领域事件与处理器", 1 if correct else 0, 1)


def module_endpoint(session: CoachSession):
    print_title("模块：Endpoint 与一致性")
    print(wrap("为外部交互设计清晰的 Endpoint：标明方法、鉴权、幂等；绑定命令或查询；必要时说明一致性策略。"))
    ans = prompt("小测：幂等的接口在重复提交时应该返回什么效果？ ")
    correct = ("相同结果" in ans) or ("无副作用" in ans) or ("idempotent" in ans.lower())
    print("参考答案：返回相同结果或不产生额外副作用。")
    session.add_note("Endpoint 与一致性", f"小测回答：{ans}；正确：{'是' if correct else '否'}")
    session.add_module("Endpoint 与一致性", 1 if correct else 0, 1)


def module_antipatterns(session: CoachSession):
    print_title("模块：反模式辨析")
    print(wrap("常见反模式：跨聚合引用、共享实体、事务跨越多个聚合、贫血模型、过度共享数据库表等。"))
    ans = prompt("小测：共享实体在 CleanDDD 中是否允许？(是/否) ")
    correct = ans in ("否", "no", "No", "NO")
    print("参考答案：否。只允许共享值对象。")
    session.add_note("反模式辨析", f"小测回答：{ans}；正确：{'是' if correct else '否'}")
    session.add_module("反模式辨析", 1 if correct else 0, 1)


def write_summary(session: CoachSession, out_path: str):
    total = session.questions if session.questions else 1
    score_pct = int(session.score * 100 / total)
    lines = []
    lines.append("# CleanDDD 教练会话笔记\n")
    lines.append(f"日期: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
    lines.append(f"总得分: {session.score}/{session.questions} ({score_pct}%)\n")
    lines.append("\n## 学习概览\n")
    lines.append("模块 | 得分 | 题数\n")
    lines.append("--- | --- | ---\n")
    for name, mscore, mqs in session.modules_done:
        lines.append(f"{name} | {mscore} | {mqs}\n")
    lines.append("\n## 模块笔记\n")
    for title, content in session.notes:
        lines.append(f"### {title}\n")
        lines.append(content + "\n\n")
    lines.append("## 行动建议\n")
    lines.append("- 若尚未进行需求拆解，建议进入 cleanddd-requirements-analysis\n")
    lines.append("- 若聚合边界已明确，建议进入 cleanddd-modeling\n")
    lines.append("- 模型稳定后可进入 cleanddd-dotnet-coding 落地实现\n")

    with open(out_path, "w", encoding="utf-8") as f:
        f.writelines(lines)
    return out_path


def main():
    print_title("CleanDDD 交互式教练")
    session = CoachSession()
    print(wrap("欢迎！请选择需要练习的模块。输入数字，多选请用逗号分隔。"))
    print("1) 总览与心智模型\n2) 聚合与不变式\n3) 命令与查询\n4) 领域事件与处理器\n5) Endpoint 与一致性\n6) 反模式辨析")
    choice = prompt("你的选择: ")
    selections = {c.strip() for c in choice.split(',') if c.strip()}

    if '1' in selections:
        module_overview(session)
    if '2' in selections:
        module_aggregate(session)
    if '3' in selections:
        module_cqrs(session)
    if '4' in selections:
        module_events(session)
    if '5' in selections:
        module_endpoint(session)
    if '6' in selections:
        module_antipatterns(session)

    out_path = prompt("笔记输出文件路径（默认 ./cleanddd-coach-notes.md）: ") or "./cleanddd-coach-notes.md"
    out_path = os.path.expanduser(out_path)
    final_path = write_summary(session, out_path)
    print(f"\n已生成笔记: {final_path}")
    print("感谢练习！建议按笔记中的行动项继续。")


if __name__ == "__main__":
    main()
