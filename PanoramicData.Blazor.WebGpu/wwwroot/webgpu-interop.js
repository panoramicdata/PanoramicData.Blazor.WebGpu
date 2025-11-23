/**
 * PanoramicData.Blazor.WebGpu JavaScript Interop
 * 
 * This file provides the minimal JavaScript layer required for WebGPU operations.
 * All WebGPU functionality is exposed through C# wrapper classes.
 * 
 * Copyright (c) 2025 Panoramic Data Limited
 * Licensed under MIT License
 */

class WebGpuInterop {
    constructor() {
        this.adapter = null;
        this.device = null;
        this.canvasContexts = new Map();
        this.resources = new Map();
        this.nextResourceId = 1;
        this.visibilityCallbacks = new Map();
        this.nextCallbackId = 1;

        // Set up visibility change listener
        this.setupVisibilityListener();
    }

    /**
     * Set up the page visibility change listener
     */
    setupVisibilityListener() {
        document.addEventListener('visibilitychange', () => {
            const isVisible = !document.hidden;
            // Notify all registered callbacks
            this.visibilityCallbacks.forEach(callback => {
                try {
                    callback.invokeMethodAsync('OnVisibilityChanged', isVisible);
                } catch (error) {
                    console.error('Error invoking visibility callback:', error);
                }
            });
        });
    }

    /**
     * Register a .NET callback for visibility changes
     * @param {object} dotNetRef - The .NET object reference
     * @returns {number} Callback ID for later removal
     */
    registerVisibilityCallback(dotNetRef) {
        const callbackId = this.nextCallbackId++;
        this.visibilityCallbacks.set(callbackId, dotNetRef);
        
        // Immediately notify of current visibility state
        const isVisible = !document.hidden;
        dotNetRef.invokeMethodAsync('OnVisibilityChanged', isVisible);
        
        return callbackId;
    }

    /**
     * Unregister a visibility callback
     * @param {number} callbackId - The callback ID to remove
     */
    unregisterVisibilityCallback(callbackId) {
        this.visibilityCallbacks.delete(callbackId);
    }

    /**
     * Check if the page is currently visible
     * @returns {boolean} True if page is visible
     */
    isPageVisible() {
        return !document.hidden;
    }

    /**
     * Check if WebGPU is supported in the current browser
     * @returns {boolean} True if WebGPU is available
     */
    isSupported() {
        return 'gpu' in navigator;
    }

    /**
     * Get detailed compatibility information about the browser and WebGPU support
     * @returns {object} Compatibility information including browser details
     */
    async getCompatibilityInfo() {
        const info = {
            isSupported: this.isSupported(),
            userAgent: navigator.userAgent || '',
            vendor: navigator.vendor || '',
            platform: navigator.platform || '',
            errorMessage: null,
            browserName: null,
            browserVersion: null,
            supportsWithFlags: false
        };

        // Detect browser
        const ua = navigator.userAgent.toLowerCase();
        if (ua.includes('chrome') && !ua.includes('edge')) {
            info.browserName = 'Chrome';
            const match = ua.match(/chrome\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = parseInt(info.browserVersion) >= 113;
        } else if (ua.includes('edg/')) {
            info.browserName = 'Edge';
            const match = ua.match(/edg\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = parseInt(info.browserVersion) >= 113;
        } else if (ua.includes('opr/') || ua.includes('opera')) {
            info.browserName = 'Opera';
            const match = ua.match(/(?:opr|opera)\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = parseInt(info.browserVersion) >= 99;
        } else if (ua.includes('firefox')) {
            info.browserName = 'Firefox';
            const match = ua.match(/firefox\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = true; // May need flags
            info.errorMessage = 'Firefox requires enabling WebGPU in about:config (dom.webgpu.enabled)';
        } else if (ua.includes('safari') && !ua.includes('chrome')) {
            info.browserName = 'Safari';
            const match = ua.match(/version\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = true; // Technology Preview
            info.errorMessage = 'Safari requires Safari Technology Preview with WebGPU enabled in Develop menu';
        } else {
            info.browserName = 'Unknown';
            info.browserVersion = 'unknown';
        }

        // Set error message if not supported
        if (!info.isSupported) {
            if (!info.errorMessage) {
                info.errorMessage = `WebGPU is not available in ${info.browserName} ${info.browserVersion}. ` +
                    `Please use Chrome 113+, Edge 113+, or Opera 99+.`;
            }
        }

        return info;
    }

    /**
     * Initialize WebGPU adapter and device
     * @param {string} canvasId - The ID of the canvas element
     * @returns {Promise<object>} Device info including adapter and device details
     */
    async initializeAsync(canvasId) {
        try {
            if (!this.isSupported()) {
                throw new Error('WebGPU is not supported in this browser');
            }

            // Request adapter
            const adapterOptions = {
                powerPreference: 'high-performance'
            };

            this.adapter = await navigator.gpu.requestAdapter(adapterOptions);
            if (!this.adapter) {
                throw new Error('Failed to get WebGPU adapter. Your GPU may not support WebGPU.');
            }

            // Request device
            const deviceDescriptor = {
                requiredFeatures: [],
                requiredLimits: {}
            };

            this.device = await this.adapter.requestDevice(deviceDescriptor);
            if (!this.device) {
                throw new Error('Failed to get WebGPU device');
            }

            // Set up device lost handler
            this.device.lost.then((info) => {
                console.error(`WebGPU device lost: ${info.message}`);
                // The C# layer will handle device lost through callbacks
            });

            // Set up uncaptured error handler
            this.device.addEventListener('uncapturederror', (event) => {
                console.error('WebGPU uncaptured error:', event.error);
            });

            // Initialize canvas context
            this.getCanvasContext(canvasId);

            return {
                adapterInfo: {
                    vendor: this.adapter.info?.vendor || 'Unknown',
                    architecture: this.adapter.info?.architecture || 'Unknown',
                    device: this.adapter.info?.device || 'Unknown',
                    description: this.adapter.info?.description || 'Unknown'
                },
                limits: this.device.limits,
                features: Array.from(this.device.features || [])
            };
        } catch (error) {
            throw new Error(`WebGPU initialization failed: ${error.message}`);
        }
    }

    /**
     * Get or create a canvas context for WebGPU rendering
     * @param {string} canvasId - The ID of the canvas element
     * @returns {object} Canvas context information
     */
    getCanvasContext(canvasId) {
        try {
            if (this.canvasContexts.has(canvasId)) {
                return { contextId: canvasId };
            }

            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                throw new Error(`Canvas element with id '${canvasId}' not found`);
            }

            const context = canvas.getContext('webgpu');
            if (!context) {
                throw new Error('Failed to get WebGPU context from canvas');
            }

            // Store the context
            this.canvasContexts.set(canvasId, context);

            return { contextId: canvasId };
        } catch (error) {
            throw new Error(`Failed to get canvas context: ${error.message}`);
        }
    }

    /**
     * Configure the canvas context
     * @param {string} contextId - The canvas context ID
     * @param {object} config - Configuration (format, usage, alphaMode, etc.)
     */
    configureCanvasContext(contextId, config) {
        try {
            const context = this.canvasContexts.get(contextId);
            if (!context) {
                throw new Error(`Canvas context '${contextId}' not found`);
            }

            const configuration = {
                device: this.device,
                format: config.format || navigator.gpu.getPreferredCanvasFormat(),
                usage: config.usage || GPUTextureUsage.RENDER_ATTACHMENT,
                alphaMode: config.alphaMode || 'opaque'
            };

            context.configure(configuration);
        } catch (error) {
            throw new Error(`Failed to configure canvas context: ${error.message}`);
        }
    }

    /**
     * Get the current texture from a canvas context
     * @param {string} contextId - The canvas context ID
     * @returns {number} Resource ID for the texture
     */
    getCurrentTexture(contextId) {
        try {
            const context = this.canvasContexts.get(contextId);
            if (!context) {
                throw new Error(`Canvas context '${contextId}' not found`);
            }

            const texture = context.getCurrentTexture();
            const resourceId = this.storeResource(texture);
            return resourceId;
        } catch (error) {
            throw new Error(`Failed to get current texture: ${error.message}`);
        }
    }

    /**
     * Create a shader module from WGSL source
     * @param {string} wgslCode - WGSL shader source code
     * @returns {Promise<number>} Resource ID for the shader module
     */
    async createShaderModuleAsync(wgslCode) {
        try {
            if (!this.device) {
                throw new Error('Device not initialized');
            }

            const shaderModule = this.device.createShaderModule({
                code: wgslCode
            });

            // Check for compilation errors
            const compilationInfo = await shaderModule.getCompilationInfo();
            if (compilationInfo.messages.length > 0) {
                const errors = compilationInfo.messages.filter(m => m.type === 'error');
                if (errors.length > 0) {
                    const errorMessages = errors.map(e => 
                        `Line ${e.lineNum}: ${e.message}`
                    ).join('\n');
                    throw new Error(`Shader compilation failed:\n${errorMessages}`);
                }
            }

            const resourceId = this.storeResource(shaderModule);
            return resourceId;
        } catch (error) {
            throw new Error(`Failed to create shader module: ${error.message}`);
        }
    }

    /**
     * Submit command buffers to the device queue
     * @param {number[]} commandBufferIds - Array of command buffer resource IDs
     */
    submitCommandBuffers(commandBufferIds) {
        try {
            if (!this.device) {
                throw new Error('Device not initialized');
            }

            const commandBuffers = commandBufferIds.map(id => {
                const buffer = this.getResource(id);
                if (!buffer) {
                    throw new Error(`Command buffer with ID ${id} not found`);
                }
                return buffer;
            });

            this.device.queue.submit(commandBuffers);
        } catch (error) {
            throw new Error(`Failed to submit command buffers: ${error.message}`);
        }
    }

    /**
     * Store a WebGPU resource and return its ID
     * @param {object} resource - The WebGPU resource to store
     * @returns {number} Resource ID
     */
    storeResource(resource) {
        const id = this.nextResourceId++;
        this.resources.set(id, resource);
        return id;
    }

    /**
     * Get a stored WebGPU resource by ID
     * @param {number} resourceId - The resource ID
     * @returns {object} The WebGPU resource
     */
    getResource(resourceId) {
        return this.resources.get(resourceId);
    }

    /**
     * Release a stored resource
     * @param {number} resourceId - The resource ID to release
     */
    releaseResource(resourceId) {
        this.resources.delete(resourceId);
    }

    /**
     * Dispose of the WebGPU device and cleanup
     */
    dispose() {
        if (this.device) {
            this.device.destroy();
            this.device = null;
        }

        this.adapter = null;
        this.canvasContexts.clear();
        this.resources.clear();
    }
}

// Create the global instance
const webGpuInterop = new WebGpuInterop();

// Export module functions for Blazor JavaScript interop
export function isSupported() {
    return webGpuInterop.isSupported();
}

export async function getCompatibilityInfo() {
    return await webGpuInterop.getCompatibilityInfo();
}

export async function initializeAsync(canvasId) {
    return await webGpuInterop.initializeAsync(canvasId);
}

export function getCanvasContext(canvasId) {
    return webGpuInterop.getCanvasContext(canvasId);
}

export function configureCanvasContext(contextId, config) {
    webGpuInterop.configureCanvasContext(contextId, config);
}

export function getCurrentTexture(contextId) {
    return webGpuInterop.getCurrentTexture(contextId);
}

export async function createShaderModuleAsync(wgslCode) {
    return await webGpuInterop.createShaderModuleAsync(wgslCode);
}

export function submitCommandBuffers(commandBufferIds) {
    webGpuInterop.submitCommandBuffers(commandBufferIds);
}

export function releaseResource(resourceId) {
    webGpuInterop.releaseResource(resourceId);
}

export function registerVisibilityCallback(dotNetRef) {
    return webGpuInterop.registerVisibilityCallback(dotNetRef);
}

export function unregisterVisibilityCallback(callbackId) {
    webGpuInterop.unregisterVisibilityCallback(callbackId);
}

export function isPageVisible() {
    return webGpuInterop.isPageVisible();
}

export function dispose() {
    webGpuInterop.dispose();
}
