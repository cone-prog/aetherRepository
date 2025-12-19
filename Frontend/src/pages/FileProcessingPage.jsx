import { FileDown, Paperclip } from "lucide-react"
import toast from "react-hot-toast";
import { useState } from "react";
import axios from "axios";

const FileProcessingPage = () => {

  const [isUploading, setIsUploading] = useState(false);
  const [fileId, setFileId] = useState(null);

  const handleFileChange = async (event) => {
    const file = event.target.files[0];
    
    if (file && file.type === "application/pdf")
    {
      setIsUploading(true);
      const formData = new FormData();
      formData.append('file', file);

      try {
        const response = await axios.post('http://localhost:5218/api/Pdf/upload', formData);
        toast.success("Upload succesful!")

        const fileId = response.data.id;
        setFileId(fileId)

      } catch (error) {
        toast.error(`Upload failed ${error}`);
      } finally {
        setIsUploading(false);
        event.target.value = null
      }
    }
    else {
      toast.error("Выберите файл PDF");
    }
  };

  const downloadProcessedFile = async () => {
    try {
      
      const response = await axios.get(`http://localhost:5218/api/Pdf/result/${fileId}/passport`);

      const jsonString = JSON.stringify(response.data, null, 2);

      const blob = new Blob([jsonString], { type: 'application/json' });

      const href = URL.createObjectURL(blob);

      const link = document.createElement('a');
      link.href = href;
      link.setAttribute('download', 'result.json'); 
      document.body.appendChild(link);
      link.click();

      document.body.removeChild(link);
      URL.revokeObjectURL(href);

    } catch (error) {
      console.error('Error downloading the file:', error);
      toast.error('Failed to download the JSON data.');
    }
  };

  return (
    <div className="w-full grow flex justify-center items-center">
      <div className="card w-4xl h-96 bg-base-100 shadow-xl rounded-4xl border-dashed border-primary border-2">
        <div className="card-body items-center justify-center gap-16">
          <h2 className="select-none card-title text-center text-4xl font-bold text-base-content">Загрузка PDF файла для парсинга</h2>
          <input
            type="file"
            id="pdf-upload"
            accept=".pdf"
            style={{ display: 'none' }} 
            onChange={handleFileChange}
          />
          {!fileId && (
          <label disabled={isUploading} htmlFor="pdf-upload" className="btn btn-primary rounded-full relative w-xl h-24">
            <Paperclip className="absolute left-6"/>
            <span className="text-2xl text-primary-content">Выбрать файл</span>
          </label>
          )}
          {fileId && (
          <button onClick={downloadProcessedFile} className="btn btn-success rounded-full relative w-xl h-24">
            <FileDown className="absolute left-6"/>
            <span className="text-2xl text-success-content">Скачать результат</span>
          </button>
          )}
        </div>
      </div>      
    </div>
  )
}

export default FileProcessingPage