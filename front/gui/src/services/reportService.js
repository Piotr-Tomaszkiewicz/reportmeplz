import api from '../api';

export const reportService = {
  getAllReports() {
    return api.get('/reports');
  },
  
  getMyReports() {
    return api.get('/reports/my');
  },

  createReport(reportData) {
    const formData = new FormData();
    formData.append('title', reportData.title);
    formData.append('description', reportData.description);
    formData.append('priority', reportData.priority);
    
    if (reportData.file) {
      formData.append('File', reportData.file); 
    }

    return api.post('/reports', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
  },

  // ZMIENIONA WERSJA: Akceptuje ID i oczekiwaną nazwę pliku
  async downloadFile(id, expectedFileName) { 
    try {
      const response = await api.get(`/reports/${id}/download-file`, {
        responseType: 'blob'
      });

      if (response.status !== 200) {
        throw new Error(`Błąd serwera podczas pobierania pliku: Status ${response.status}`);
      }
      
      const contentDisposition = response.headers['content-disposition'];
      let fileNameToSave = expectedFileName || `zalacznik-zgloszenia-${id}.bin`; 
      // ^ Domyślnie używamy nazwy z Vue, jeśli jej nie ma, używamy bezpiecznego domyślnego pliku

      // Próbujemy nadpisać nazwę z nagłówka Content-Disposition (jeśli serwer go poprawnie ustawił)
      if (contentDisposition) {
        const match = contentDisposition.match(/filename\*?=['"]?(?:UTF-8'')?([^;]+)/i);
        if (match && match[1]) {
          fileNameToSave = decodeURIComponent(match[1].replace(/"/g, ''));
        }
      }
      
      // Wymuszenie pobrania
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      
      link.href = url;
      link.setAttribute('download', fileNameToSave); // Używamy poprawnie ustalonej nazwy
      
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
    } catch (error) {
      if (error.response) {
        throw new Error(`Serwer zwrócił błąd ${error.response.status}. Brak dostępu lub plik nie istnieje.`);
      }
      throw error; 
    }
  }
};