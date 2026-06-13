/**
 * Chrome 109（Win7 最后版本）缺少 ES2023 不可变数组方法，Vben 布局/标签栏会调用 toSorted/toReversed。
 * @see https://caniuse.com/mdn-javascript_builtins_array_tosorted
 */
function patchImmutableArrayMethods() {
  if (!Array.prototype.toSorted) {
    Array.prototype.toSorted = function toSorted<T>(
      this: T[],
      compareFn?: (a: T, b: T) => number,
    ) {
      return [...this].sort(compareFn);
    };
  }

  if (!Array.prototype.toReversed) {
    Array.prototype.toReversed = function toReversed<T>(this: T[]) {
      return [...this].reverse();
    };
  }
}

patchImmutableArrayMethods();
