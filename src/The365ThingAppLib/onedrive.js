window.triggerFileDownload = function (fileName, fileContent) {
    const blob = new Blob([new Uint8Array(fileContent)], { type: 'application/octet-stream' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

window.readFileAsBytes = async function (fileInputRef) {
    const files = fileInputRef.value?.files;
    if (!files || files.length === 0) return null;

    const file = files[0];
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = (e) => {
            const bytes = new Uint8Array(e.target.result);
            resolve(Array.from(bytes));
        };
        reader.onerror = reject;
        reader.readAsArrayBuffer(file);
    });
};

window.getFileName = function (fileInputRef) {
    const files = fileInputRef.value?.files;
    if (!files || files.length === 0) return '';
    return files[0].name;
};

window.closeModal = function (modalId) {
    const modalElement = document.getElementById(modalId);
    if (modalElement) {
        const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
        modal.hide();
    }
};
