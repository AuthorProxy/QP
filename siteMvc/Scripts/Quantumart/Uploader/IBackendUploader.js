window.EVENT_TYPE_LIBRARY_FILE_UPLOADED = 'OnLibraryFileUploaded';
window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED = 'OnLibraryAllFilesUploaded';

Quantumart.QP8.IBackendUploader = function () { };
Quantumart.QP8.IBackendUploader.prototype = {
  initialize () {
    $q.alertFail('initialize is not implemented');
  },
  dispose () {
    $q.alertFail('dispose is not implemented');
  },
  set_folderPath () {
    $q.alertFail('set_folderPath is not implemented');
  },
  get_folderPath () {
    $q.alertFail('get_folderPath is not implemented');
  }
};

Quantumart.QP8.IBackendUploader.registerInterface('Quantumart.QP8.IBackendUploader');
Quantumart.QP8.BackendUploaderEventArgs = function (fileNames) {
  Quantumart.QP8.BackendUploaderEventArgs.initializeBase(this);

  this._fileNames = fileNames;
};

Quantumart.QP8.BackendUploaderEventArgs.prototype = {
  _fileNames: [],

  get_fileNames () {
    return this._fileNames;
  }
};

Quantumart.QP8.BackendUploaderEventArgs.registerClass('Quantumart.QP8.BackendUploaderEventArgs', Sys.EventArgs);
