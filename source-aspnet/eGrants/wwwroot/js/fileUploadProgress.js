/**
 * File Upload Progress Manager with SignalR integration
 */
class FileUploadProgressManager {
    constructor() {
        this.connection = null;
        this.activeUploads = new Map();
        this.initialize();
    }

    /**
     * Initialize SignalR connection
     */
    async initialize() {
        try {
            // Create SignalR connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/fileupload")
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Handle progress updates
            this.connection.on("UploadProgress", (progress) => {
                this.handleProgressUpdate(progress);
            });

            // Handle reconnection
            this.connection.onreconnecting(() => {
                console.log("SignalR reconnecting...");
            });

            this.connection.onreconnected(() => {
                console.log("SignalR reconnected.");
                // Rejoin all active upload groups
                this.activeUploads.forEach((_, uploadId) => {
                    this.connection.invoke("JoinUploadGroup", uploadId);
                });
            });

            // Start connection
            await this.connection.start();
            console.log("SignalR connected for file upload progress.");
        } catch (error) {
            console.error("Failed to initialize SignalR connection:", error);
        }
    }

    /**
     * Start tracking an upload
     */
    async startUpload(uploadId, fileName) {
        if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
            await this.initialize();
        }

        // Request notification permission
        await this.requestNotificationPermission();

        // Join the upload group
        await this.connection.invoke("JoinUploadGroup", uploadId);

        // Store upload info
        this.activeUploads.set(uploadId, { fileName, startTime: Date.now() });

        // Show progress modal
        this.showProgressModal(uploadId, fileName);
    }

    /**
     * Handle progress update from SignalR
     */
    handleProgressUpdate(progress) {
        console.log("Upload progress update:", progress);

        const modalId = `upload-progress-${progress.uploadId}`;
        const modal = document.getElementById(modalId);

        if (!modal) {
            console.warn("Progress modal not found for uploadId:", progress.uploadId);
            return;
        }

        // Update progress bar
        const progressBar = modal.querySelector('.upload-progress-bar');
        if (progressBar) {
            progressBar.style.width = `${progress.percentComplete}%`;
            progressBar.setAttribute('aria-valuenow', progress.percentComplete);
            progressBar.textContent = `${progress.percentComplete}%`;
        }

        // Update status message
        const statusMessage = modal.querySelector('.upload-status-message');
        if (statusMessage) {
            statusMessage.textContent = progress.message;
        }

        // Update status badge
        const statusBadge = modal.querySelector('.upload-status-badge');
        if (statusBadge) {
            statusBadge.className = 'badge upload-status-badge ' + this.getStatusBadgeClass(progress.status);
            statusBadge.textContent = this.getStatusText(progress.status);
        }

        // Handle completion
        if (progress.status === 'Completed') {
            this.handleUploadComplete(progress);
        } else if (progress.status === 'Failed') {
            this.handleUploadFailed(progress);
        }
    }

    /**
     * Show progress modal
     */
    showProgressModal(uploadId, fileName) {
        const modalId = `upload-progress-${uploadId}`;

        // Check if modal already exists
        if (document.getElementById(modalId)) {
            return;
        }

        const modalHtml = `
            <div class="modal fade" id="${modalId}" tabindex="-1" role="dialog" data-backdrop="static" data-keyboard="false">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">
                                <i class="fas fa-cloud-upload-alt"></i> File Upload Progress
                            </h5>
                        </div>
                        <div class="modal-body">
                            <p><strong>File:</strong> ${fileName}</p>
                            <div class="upload-status-container mb-3">
                                <span class="badge upload-status-badge badge-info">Uploading</span>
                            </div>
                            <div class="progress" style="height: 25px;">
                                <div class="progress-bar progress-bar-striped progress-bar-animated upload-progress-bar" 
                                     role="progressbar" 
                                     aria-valuenow="0" 
                                     aria-valuemin="0" 
                                     aria-valuemax="100" 
                                     style="width: 0%;">
                                    0%
                                </div>
                            </div>
                            <p class="upload-status-message mt-3 text-muted">Initializing upload...</p>
                            <div class="upload-actions mt-3" style="display: none;">
                                <button type="button" class="btn btn-primary btn-view-document" style="display: none;">
                                    <i class="fas fa-eye"></i> View Document
                                </button>
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-danger btn-cancel-upload" data-upload-id="${uploadId}">
                                <i class="fas fa-times"></i> Cancel Upload
                            </button>
                            <small class="text-muted ml-3">You can leave this page and continue working. We'll notify you when complete.</small>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Add modal to page
        document.body.insertAdjacentHTML('beforeend', modalHtml);

        // Show modal
        $(`#${modalId}`).modal('show');

        // Add cancel handler
        document.querySelector(`#${modalId} .btn-cancel-upload`).addEventListener('click', () => {
            this.cancelUpload(uploadId);
        });
    }

    /**
     * Handle upload completion
     */
    handleUploadComplete(progress) {
        const modalId = `upload-progress-${progress.uploadId}`;
        const modal = document.getElementById(modalId);

        if (modal) {
            // Hide cancel button
            const cancelBtn = modal.querySelector('.btn-cancel-upload');
            if (cancelBtn) {
                cancelBtn.style.display = 'none';
            }

            // Show action buttons
            const actionsDiv = modal.querySelector('.upload-actions');
            if (actionsDiv) {
                actionsDiv.style.display = 'block';
            }

            // Setup view document button
            if (progress.documentUrl) {
                const viewBtn = modal.querySelector('.btn-view-document');
                if (viewBtn) {
                    viewBtn.style.display = 'inline-block';
                    viewBtn.onclick = () => {
                        window.open(progress.documentUrl, '_blank');
                    };
                }
            }

            // Show notification
            this.showNotification('Upload Complete', 
                `File "${progress.fileName}" has been uploaded successfully.`, 
                'success');

            // Leave upload group
            this.connection.invoke("LeaveUploadGroup", progress.uploadId);
            this.activeUploads.delete(progress.uploadId);
        }
    }

    /**
     * Handle upload failure
     */
    handleUploadFailed(progress) {
        const modalId = `upload-progress-${progress.uploadId}`;
        const modal = document.getElementById(modalId);

        if (modal) {
            // Hide cancel button
            const cancelBtn = modal.querySelector('.btn-cancel-upload');
            if (cancelBtn) {
                cancelBtn.style.display = 'none';
            }

            // Show close button
            const actionsDiv = modal.querySelector('.upload-actions');
            if (actionsDiv) {
                actionsDiv.style.display = 'block';
            }

            // Show notification
            this.showNotification('Upload Failed', 
                `Failed to upload "${progress.fileName}": ${progress.message}`, 
                'error');

            // Leave upload group
            this.connection.invoke("LeaveUploadGroup", progress.uploadId);
            this.activeUploads.delete(progress.uploadId);
        }
    }

    /**
     * Cancel an upload
     */
    async cancelUpload(uploadId) {
        try {
            const response = await fetch('/EgrantsDoc/CancelUpload', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ uploadId: uploadId })
            });

            // Check if response is OK before parsing JSON
            if (!response.ok) {
                // Try to get error message from response
                let errorMessage = `Server returned ${response.status}: ${response.statusText}`;
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorMessage;
                } catch {
                    // If response isn't JSON, use status text
                }
                throw new Error(errorMessage);
            }

            const result = await response.json();

            if (result.success) {
                const modalId = `upload-progress-${uploadId}`;
                $(`#${modalId}`).modal('hide');
                this.showNotification('Upload Cancelled', 'The file upload has been cancelled.', 'info');
            } else {
                this.showNotification('Cancel Failed', result.message || 'Failed to cancel upload.', 'error');
            }
        } catch (error) {
            console.error("Failed to cancel upload:", error);
            this.showNotification('Cancel Failed', error.message || 'An error occurred while cancelling the upload.', 'error');
        }
    }

    /**
     * Get status badge CSS class
     */
    getStatusBadgeClass(status) {
        const classes = {
            'Queued': 'badge-secondary',
            'Uploading': 'badge-info',
            'Completed': 'badge-success',
            'Failed': 'badge-danger',
            'Cancelled': 'badge-warning'
        };
        return classes[status] || 'badge-secondary';
    }

    /**
     * Get status display text
     */
    getStatusText(status) {
        const texts = {
            'Queued': 'Queued',
            'Uploading': 'Uploading',
            'Completed': 'Completed',
            'Failed': 'Failed',
            'Cancelled': 'Cancelled'
        };
        return texts[status] || status;
    }

    /**
     * Show browser notification
     */
    showNotification(title, message, type = 'info') {
        // Try browser notification first
        if ('Notification' in window && Notification.permission === 'granted') {
            new Notification(title, {
                body: message,
                icon: type === 'success' ? '/images/success-icon.png' : 
                      type === 'error' ? '/images/error-icon.png' : 
                      '/images/info-icon.png'
            });
        }

        // Fallback to Bootstrap toast/alert
        const toastClass = type === 'success' ? 'alert-success' : 
                          type === 'error' ? 'alert-danger' : 
                          'alert-info';

        const toastHtml = `
            <div class="alert ${toastClass} alert-dismissible fade show upload-toast" 
                 role="alert" 
                 style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
                <strong>${title}</strong> ${message}
                <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', toastHtml);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            const toast = document.querySelector('.upload-toast');
            if (toast) {
                $(toast).alert('close');
            }
        }, 5000);
    }

    /**
     * Request notification permission on first upload
     */
    async requestNotificationPermission() {
        if ('Notification' in window && Notification.permission === 'default') {
            await Notification.requestPermission();
        }
    }
}

// Global instance
window.fileUploadProgressManager = new FileUploadProgressManager();

// Request notification permission on page load
if ("Notification" in window && Notification.permission === "default") {
    Notification.requestPermission();
}