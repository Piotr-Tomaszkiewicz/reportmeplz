import api from '../api';

export const commentService = {
  // Pobierz listę komentarzy dla danego zgłoszenia
  getCommentsByReportId(reportId) {
    return api.get(`/comments/report/${reportId}`);
  },

  // Dodaj nowy komentarz (FormData ze względu na opcjonalny plik)
  addComment(commentData) {
    const formData = new FormData();
    formData.append('content', commentData.content);
    formData.append('reportId', commentData.reportId);
    
    if (commentData.file) {
      formData.append('file', commentData.file);
    }

    return api.post('/comments', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  },

  // Pobierz załącznik z komentarza
  async downloadCommentFile(commentId, fileName) {
    try {
      const response = await api.get(`/comments/${commentId}/download-file`, {
        responseType: 'blob'
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName || `zalacznik-${commentId}.bin`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch (error) {
      console.error("Błąd pobierania pliku komentarza:", error);
      throw error;
    }
  }
};