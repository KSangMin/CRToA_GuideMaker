mergeInto(LibraryManager.library, {
    DownloadWebGLFile: function (array, size, fileName, contentType) {
        var bytes = new Uint8Array(size);
        for (var i = 0; i < size; i++) {
            bytes[i] = HEAPU8[array + i];
        }

        // Pointer_stringify 대신 UTF8ToString을 사용합니다.
        var blob = new Blob([bytes], { type: UTF8ToString(contentType) });
        var link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = UTF8ToString(fileName);
        link.click();
        window.URL.revokeObjectURL(link.href);
    }
});