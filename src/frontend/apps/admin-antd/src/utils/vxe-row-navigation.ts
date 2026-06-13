export function shouldIgnoreVxeCellDblclick(column: { field?: string; type?: string } | undefined) {
  return column?.type === 'checkbox' || column?.field === 'operation';
}

export function handleVxeCellDblclick<T>(
  event: { column?: { field?: string; type?: string }; row?: T },
  handler: (row: T) => void | Promise<void>,
) {
  if (shouldIgnoreVxeCellDblclick(event.column) || !event.row) return;
  void handler(event.row);
}
