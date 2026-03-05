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
  xlsx: 'excel',
  jpg: 'image',
  jpeg: 'image',
  png: 'image',
  gif: 'image',
  bmp: 'image',
  txt: 'text',
};

export function getFilePreviewType(fileName: string): FilePreviewType {
  const ext = fileName.split('.').pop()?.toLowerCase() ?? '';
  return EXT_MAP[ext] ?? 'unsupported';
}
