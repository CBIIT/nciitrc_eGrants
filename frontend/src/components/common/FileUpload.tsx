"use client";

import { useRef, useState } from "react";

interface FileUploadProps {
  onUpload: (file: File) => Promise<void>;
  accept?: string;
  label?: string;
  disabled?: boolean;
}

export default function FileUpload({
  onUpload,
  accept = ".pdf,.doc,.docx,.msg,.rtf,.jpg,.png,.gif,.tif,.html,.htm,.log,.dat,.txt",
  label = "Upload File",
  disabled = false,
}: FileUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);
  const [fileName, setFileName] = useState("");

  async function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;

    setFileName(file.name);
    setUploading(true);
    try {
      await onUpload(file);
    } finally {
      setUploading(false);
      if (inputRef.current) inputRef.current.value = "";
    }
  }

  return (
    <div className="flex items-center gap-3">
      <input
        ref={inputRef}
        type="file"
        accept={accept}
        onChange={handleChange}
        disabled={disabled || uploading}
        className="hidden"
        id="file-upload"
      />
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        disabled={disabled || uploading}
        className="btn-secondary"
      >
        {uploading ? "Uploading..." : label}
      </button>
      {fileName && (
        <span className="text-sm text-text-muted">{fileName}</span>
      )}
    </div>
  );
}
