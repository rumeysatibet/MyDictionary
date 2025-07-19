// Mesaj konteynırını en alta scroll yapar
window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

// Konteynır scroll pozisyonunu kontrol eder
window.isScrolledToBottom = (element) => {
    if (!element) return true;
    return element.scrollHeight - element.clientHeight <= element.scrollTop + 1;
};

// Auto-resize textarea
window.autoResizeTextarea = (element) => {
    if (element) {
        element.style.height = 'auto';
        element.style.height = (element.scrollHeight) + 'px';
    }
};