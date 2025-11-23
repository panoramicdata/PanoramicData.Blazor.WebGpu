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
    }

    /**
     * Check if WebGPU is supported in the current browser
     * @returns {boolean} True if WebGPU is available
     */
    isSupported() {
        return 'gpu' in navigator;
    }

    /**
     * Get detailed browser compatibility information
     * @returns {object} Browser and WebGPU support details
     */
    getCompatibilityInfo() {
        const info = {
            isSupported: this.isSupported(),
            userAgent: navigator.userAgent,
            vendor: navigator.vendor,
            platform: navigator.platform
        };

        if (!info.isSupported) {
            info.errorMessage = 'WebGPU is not supported in this browser. ' +
                'Please use Chrome 113+, Edge 113+, or Opera 99+ with WebGPU enabled.';
        }

        return info;
    }

    /**
     * Initialize WebGPU adapter and device
     * @param {object} options - Initialization options (powerPreference, etc.)
     * @returns {Promise<object>} Device information
     */
    async initializeAsync(options = {}) {
        try {
            if (!this.isSupported()) {
                throw new Error('WebGPU is not supported in this browser');
            }

            // Request adapter
            const adapterOptions = {
                powerPreference: options.powerPreference || 'high-performance'
            };

            this.adapter = await navigator.gpu.requestAdapter(adapterOptions);
            if (!this.adapter) {
                throw new Error('Failed to get WebGPU adapter. Your GPU may not support WebGPU.');
            }

            // Request device
            const deviceDescriptor = {
                requiredFeatures: options.requiredFeatures || [],
                requiredLimits: options.requiredLimits || {}
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

// Create and export the global instance
window.webGpuInterop = new WebGpuInterop();

// Export for ES6 module usage
export default window.webGpuInterop;
