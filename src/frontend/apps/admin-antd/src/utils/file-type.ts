export type FilePreviewType =
  | 'pdf'
  | 'docx'
  | 'excel'
  | 'image'
  | 'text'
  | 'unsupported';

const EXT_MAP: Record<string, FilePreviewType> = {
  pdf: 'pdf',
  docx: 'docx',
  xls: 'excel',
  xlsx: 'excel',
  jpg: 'image',
  jpeg: 'image',
  png: 'image',
  gif: 'image',
  bmp: 'image',
  webp: 'image',
  csv: 'text',
  log: 'text',
  txt: 'text',
};

/** 根据文件名判断前端预览组件类型。 */
export function getFilePreviewType(fileName: string): FilePreviewType {
  const ext = fileName.split('.').pop()?.toLowerCase() ?? '';
  return EXT_MAP[ext] ?? 'unsupported';
}
