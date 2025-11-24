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

        // Error tracking
        this.errorCounts = new Map(); // Map of error message -> count
        this.lastErrorReport = Date.now();
        this.errorReportInterval = 5000; // Report every 5 seconds
        this.maxUniqueErrorsToTrack = 50; // Prevent unbounded growth

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
            vendor: '', // Will be populated from userAgent
            platform: navigator.userAgentData?.platform || this.getPlatformFromUserAgent(),
            errorMessage: null,
            browserName: null,
            browserVersion: null,
            supportsWithFlags: false
        };

        // Detect browser and vendor from userAgent
        const ua = navigator.userAgent.toLowerCase();
        if (ua.includes('chrome') && !ua.includes('edge')) {
            info.browserName = 'Chrome';
            info.vendor = 'Google Inc.';
            const match = ua.match(/chrome\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = parseInt(info.browserVersion) >= 113;
        } else if (ua.includes('edg/')) {
            info.browserName = 'Edge';
            info.vendor = 'Microsoft Corporation';
            const match = ua.match(/edg\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = parseInt(info.browserVersion) >= 113;
        } else if (ua.includes('opr/') || ua.includes('opera')) {
            info.browserName = 'Opera';
            info.vendor = 'Opera Software';
            const match = ua.match(/(?:opr|opera)\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = parseInt(info.browserVersion) >= 99;
        } else if (ua.includes('firefox')) {
            info.browserName = 'Firefox';
            info.vendor = 'Mozilla';
            const match = ua.match(/firefox\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = true; // May need flags
            info.errorMessage = 'Firefox requires enabling WebGPU in about:config (dom.webgpu.enabled)';
        } else if (ua.includes('safari') && !ua.includes('chrome')) {
            info.browserName = 'Safari';
            info.vendor = 'Apple Inc.';
            const match = ua.match(/version\/(\d+)/);
            info.browserVersion = match ? match[1] : 'unknown';
            info.supportsWithFlags = true; // Technology Preview
            info.errorMessage = 'Safari requires Safari Technology Preview with WebGPU enabled in Develop menu';
        } else {
            info.browserName = 'Unknown';
            info.vendor = 'Unknown';
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
     * Get platform information from userAgent as fallback
     * @returns {string} Platform identifier
     */
    getPlatformFromUserAgent() {
        const ua = navigator.userAgent;
        if (ua.includes('Win')) return 'Windows';
        if (ua.includes('Mac')) return 'macOS';
        if (ua.includes('Linux')) return 'Linux';
        if (ua.includes('Android')) return 'Android';
        if (ua.includes('iPhone') || ua.includes('iPad')) return 'iOS';
        return 'Unknown';
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

            // Set up uncaptured error handler with tracking
            this.device.addEventListener('uncapturederror', (event) => {
                this.trackError('WebGPU uncaptured error', event.error);
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
     * Configure a canvas context with proper resolution
     * @param {string} contextId - The context ID
     * @param {object} config - Configuration options
     */
    async configureCanvasContext(contextId, config) {
        try {
            const context = this.canvasContexts.get(contextId);
            if (!context) {
                throw new Error(`Canvas context '${contextId}' not found`);
            }

            // Get the canvas element
            const canvas = context.canvas;
            
            // Set canvas resolution to match display size with device pixel ratio
            const dpr = window.devicePixelRatio || 1;
            const displayWidth = canvas.clientWidth;
            const displayHeight = canvas.clientHeight;
            
            // Set the internal canvas resolution
            canvas.width = displayWidth * dpr;
            canvas.height = displayHeight * dpr;

            // Configure WebGPU context
            const configuration = {
                device: this.device,
                format: config.format || navigator.gpu.getPreferredCanvasFormat(),
                alphaMode: config.alphaMode || 'opaque'
            };

            context.configure(configuration);
            
            console.log(`Canvas configured: ${canvas.width}x${canvas.height} (display: ${displayWidth}x${displayHeight}, dpr: ${dpr})`);
        } catch (error) {
            throw new Error(`Failed to configure canvas context: ${error.message}`);
        }
    }

    /**
     * Get a canvas context for WebGPU rendering
     * @param {string} canvasId - The canvas element ID
     * @returns {string} Context ID for later reference
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
     * Create a texture view from a texture
     * @param {number} textureId - The texture resource ID
     * @param {object} descriptor - Optional view descriptor
     * @returns {number} Resource ID for the texture view
     */
    createTextureView(textureId, descriptor) {
        try {
            const texture = this.getResource(textureId);
            if (!texture) {
                throw new Error(`Texture with ID ${textureId} not found`);
            }

            const view = texture.createView(descriptor);
            return this.storeResource(view);
        } catch (error) {
            throw new Error(`Failed to create texture view: ${error.message}`);
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
                    const error = new Error(`Shader compilation failed:\n${errorMessages}`);
                    this.trackError('Shader compilation error', error);
                    throw error;
                }
                
                // Log warnings but don't fail
                const warnings = compilationInfo.messages.filter(m => m.type === 'warning');
                if (warnings.length > 0) {
                    const warnMessages = warnings.map(w => 
                        `Line ${w.lineNum}: ${w.message}`
                    ).join('\n');
                    console.warn(`Shader compilation warnings:\n${warnMessages}`);
                }
            }

            const resourceId = this.storeResource(shaderModule);
            return resourceId;
        } catch (error) {
            if (error.message && !error.message.includes('Shader compilation failed')) {
                this.trackError('Failed to create shader module', error);
            }
            throw error;
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
        // Report any remaining errors before disposing
        if (this.errorCounts.size > 0) {
            this.reportErrorSummary();
        }

        if (this.device) {
            this.device.destroy();
            this.device = null;
        }

        this.adapter = null;
        this.canvasContexts.clear();
        this.resources.clear();
        this.visibilityCallbacks.clear();
        this.errorCounts.clear();
    }

    /**
     * Create a buffer with initial data
     * @param {ArrayBuffer} data - The buffer data
     * @param {number} usage - Buffer usage flags
     * @param {string} label - Optional label for debugging
     * @returns {number} Resource ID for the buffer
     */
    createBuffer(data, usage, label) {
        try {
            if (!this.device) {
                throw new Error('Device not initialized');
            }

            console.log(`Creating buffer: ${label || 'unnamed'}, size: ${data.byteLength} bytes, usage: ${usage}`);

            const buffer = this.device.createBuffer({
                label: label || undefined,
                size: data.byteLength,
                usage: usage | GPUBufferUsage.COPY_DST,
                mappedAtCreation: true
            });

            // Copy data to buffer
            const mappedRange = buffer.getMappedRange();
            new Uint8Array(mappedRange).set(new Uint8Array(data));
            buffer.unmap();

            const resourceId = this.storeResource(buffer);
            console.log(`Created buffer ${label || 'unnamed'} with resource ID: ${resourceId}`);
            return resourceId;
        } catch (error) {
            console.error(`Failed to create buffer ${label || 'unnamed'}:`, error);
            throw new Error(`Failed to create buffer: ${error.message}`);
        }
    }

    /**
     * Write data to an existing buffer
     * @param {number} bufferId - The buffer resource ID
     * @param {ArrayBuffer} data - The data to write
     * @param {number} offset - Offset in bytes
     */
    writeBuffer(bufferId, data, offset) {
        try {
            const buffer = this.getResource(bufferId);
            if (!buffer) {
                throw new Error(`Buffer with ID ${bufferId} not found`);
            }

            this.device.queue.writeBuffer(buffer, offset, data);
        } catch (error) {
            throw new Error(`Failed to write buffer: ${error.message}`);
        }
    }

    /**
     * Create a render pipeline
     * @param {object} descriptor - Pipeline descriptor
     * @returns {number} Resource ID for the pipeline
     */
    createRenderPipeline(descriptor) {
        try {
            if (!this.device) {
                throw new Error('Device not initialized');
            }

            // Convert shader resource IDs to actual shader modules
            if (!descriptor.vertex || !descriptor.vertex.shaderModuleId) {
                throw new Error('Vertex shader module ID is required');
            }

            const vertexModule = this.getResource(descriptor.vertex.shaderModuleId);
            if (!vertexModule) {
                throw new Error(`Vertex shader module with ID ${descriptor.vertex.shaderModuleId} not found`);
            }

            const pipelineDescriptor = {
                label: descriptor.label || undefined,
                layout: 'auto',
                vertex: {
                    module: vertexModule,
                    entryPoint: descriptor.vertex.entryPoint || 'main',
                    buffers: descriptor.vertex.buffers || []
                }
            };

            // Add fragment stage if provided
            if (descriptor.fragment) {
                if (!descriptor.fragment.shaderModuleId) {
                    throw new Error('Fragment shader module ID is required when fragment stage is specified');
                }

                const fragmentModule = this.getResource(descriptor.fragment.shaderModuleId);
                if (!fragmentModule) {
                    throw new Error(`Fragment shader module with ID ${descriptor.fragment.shaderModuleId} not found`);
                }

                pipelineDescriptor.fragment = {
                    module: fragmentModule,
                    entryPoint: descriptor.fragment.entryPoint || 'main',
                    targets: descriptor.fragment.targets || []
                };
            }

            // Add optional states
            if (descriptor.primitive) {
                pipelineDescriptor.primitive = descriptor.primitive;
            }
            if (descriptor.depthStencil) {
                pipelineDescriptor.depthStencil = descriptor.depthStencil;
            }
            if (descriptor.multisample) {
                pipelineDescriptor.multisample = descriptor.multisample;
            }

            console.log('Creating render pipeline with descriptor:', pipelineDescriptor);
            const pipeline = this.device.createRenderPipeline(pipelineDescriptor);
            const resourceId = this.storeResource(pipeline);
            console.log('Created render pipeline with resource ID:', resourceId);
            return resourceId;
        } catch (error) {
            this.trackError('Failed to create render pipeline', error);
            throw error;
        }
    }

    /**
     * Create a bind group
     * @param {object} descriptor - Bind group descriptor
     * @returns {number} Resource ID for the bind group
     */
    createBindGroup(descriptor) {
        try {
            if (!this.device) {
                throw new Error('Device not initialized');
            }

            // Get the layout (either from a pipeline or explicit layout ID)
            let layout;
            if (descriptor.layoutId !== null && descriptor.layoutId !== undefined) {
                layout = this.getResource(descriptor.layoutId);
            } else if (descriptor.pipelineId !== null && descriptor.pipelineId !== undefined) {
                // Get layout from pipeline
                const pipeline = this.getResource(descriptor.pipelineId);
                if (!pipeline) {
                    throw new Error(`Pipeline with ID ${descriptor.pipelineId} not found`);
                }
                layout = pipeline.getBindGroupLayout(descriptor.groupIndex || 0);
            } else {
                throw new Error('Must specify either layoutId or pipelineId');
            }

            // Convert resource IDs to actual resources
            const entries = descriptor.entries.map(entry => {
                const resource = this.getResource(entry.resourceId);
                return {
                    binding: entry.binding,
                    resource: entry.resourceType === 'buffer' 
                        ? { buffer: resource }
                        : resource
                };
            });

            const bindGroupDescriptor = {
                label: descriptor.label || undefined,
                layout: layout,
                entries: entries
            };

            const bindGroup = this.device.createBindGroup(bindGroupDescriptor);
            return this.storeResource(bindGroup);
        } catch (error) {
            throw new Error(`Failed to create bind group: ${error.message}`);
        }
    }

    /**
     * Create a command encoder
     * @param {string} label - Optional label for debugging
     * @returns {number} Resource ID for the command encoder
     */
    createCommandEncoder(label) {
        try {
            if (!this.device) {
                throw new Error('Device not initialized');
            }

            const encoder = this.device.createCommandEncoder({
                label: label || undefined
            });

            return this.storeResource(encoder);
        } catch (error) {
            throw new Error(`Failed to create command encoder: ${error.message}`);
        }
    }

    /**
     * Begin a render pass
     * @param {number} encoderId - Command encoder resource ID
     * @param {object} descriptor - Render pass descriptor
     * @returns {number} Resource ID for the render pass encoder
     */
    beginRenderPass(encoderId, descriptor) {
        try {
            const encoder = this.getResource(encoderId);
            if (!encoder) {
                throw new Error(`Command encoder with ID ${encoderId} not found`);
            }

            // Convert resource IDs to actual resources
            const colorAttachments = descriptor.colorAttachments.map(att => {
                const view = att.viewId !== undefined 
                    ? this.getResource(att.viewId)
                    : undefined;
                
                return {
                    view: view,
                    resolveTarget: att.resolveTargetId !== undefined
                        ? this.getResource(att.resolveTargetId)
                        : undefined,
                    loadOp: att.loadOp,
                    storeOp: att.storeOp,
                    clearValue: att.clearValue
                };
            });

            const passDescriptor = {
                label: descriptor.label || undefined,
                colorAttachments: colorAttachments
            };

            // Add depth/stencil if provided
            if (descriptor.depthStencilAttachment) {
                const ds = descriptor.depthStencilAttachment;
                passDescriptor.depthStencilAttachment = {
                    view: this.getResource(ds.viewId),
                    depthLoadOp: ds.depthLoadOp,
                    depthStoreOp: ds.depthStoreOp,
                    depthClearValue: ds.depthClearValue,
                    stencilLoadOp: ds.stencilLoadOp,
                    stencilStoreOp: ds.stencilStoreOp,
                    stencilClearValue: ds.stencilClearValue
                };
            }

            const passEncoder = encoder.beginRenderPass(passDescriptor);
            return this.storeResource(passEncoder);
        } catch (error) {
            this.trackError('Failed to begin render pass', error);
            throw error;
        }
    }

    /**
     * Set the pipeline for a render pass
     * @param {number} passEncoderId - Render pass encoder resource ID
     * @param {number} pipelineId - Pipeline resource ID
     */
    setPipeline(passEncoderId, pipelineId) {
        try {
            const passEncoder = this.getResource(passEncoderId);
            const pipeline = this.getResource(pipelineId);
            
            if (!passEncoder) {
                throw new Error(`Render pass encoder with ID ${passEncoderId} not found`);
            }
            if (!pipeline) {
                throw new Error(`Pipeline with ID ${pipelineId} not found`);
            }

            passEncoder.setPipeline(pipeline);
        } catch (error) {
            this.trackError('Failed to set pipeline', error);
            throw error;
        }
    }

    /**
     * Set a bind group for a render pass
     * @param {number} passEncoderId - Render pass encoder resource ID
     * @param {number} index - Bind group index
     * @param {number} bindGroupId - Bind group resource ID
     */
    setBindGroup(passEncoderId, index, bindGroupId) {
        try {
            const passEncoder = this.getResource(passEncoderId);
            const bindGroup = this.getResource(bindGroupId);
            
            if (!passEncoder) {
                throw new Error(`Render pass encoder with ID ${passEncoderId} not found`);
            }
            if (!bindGroup) {
                throw new Error(`Bind group with ID ${bindGroupId} not found`);
            }

            passEncoder.setBindGroup(index, bindGroup);
        } catch (error) {
            throw new Error(`Failed to set bind group: ${error.message}`);
        }
    }

    /**
     * Set a vertex buffer for a render pass
     * @param {number} passEncoderId - Render pass encoder resource ID
     * @param {number} slot - Vertex buffer slot
     * @param {number} bufferId - Buffer resource ID
     */
    setVertexBuffer(passEncoderId, slot, bufferId) {
        try {
            const passEncoder = this.getResource(passEncoderId);
            const buffer = this.getResource(bufferId);
            
            if (!passEncoder) {
                throw new Error(`Render pass encoder with ID ${passEncoderId} not found`);
            }
            if (!buffer) {
                throw new Error(`Buffer with ID ${bufferId} not found`);
            }

            passEncoder.setVertexBuffer(slot, buffer);
        } catch (error) {
            throw new Error(`Failed to set vertex buffer: ${error.message}`);
        }
    }

    /**
     * Set an index buffer for a render pass
     * @param {number} passEncoderId - Render pass encoder resource ID
     * @param {number} bufferId - Buffer resource ID
     * @param {string} format - Index format ('uint16' or 'uint32')
     */
    setIndexBuffer(passEncoderId, bufferId, format) {
        try {
            const passEncoder = this.getResource(passEncoderId);
            const buffer = this.getResource(bufferId);
            
            if (!passEncoder) {
                throw new Error(`Render pass encoder with ID ${passEncoderId} not found`);
            }
            if (!buffer) {
                throw new Error(`Buffer with ID ${bufferId} not found`);
            }

            passEncoder.setIndexBuffer(buffer, format);
        } catch (error) {
            throw new Error(`Failed to set index buffer: ${error.message}`);
        }
    }

    /**
     * Draw vertices
     * @param {number} passEncoderId - Render pass encoder resource ID
     * @param {number} vertexCount - Number of vertices to draw
     * @param {number} instanceCount - Number of instances to draw
     * @param {number} firstVertex - First vertex index
     * @param {number} firstInstance - First instance index
     */
    draw(passEncoderId, vertexCount, instanceCount, firstVertex, firstInstance) {
        try {
            const passEncoder = this.getResource(passEncoderId);
            if (!passEncoder) {
                throw new Error(`Render pass encoder with ID ${passEncoderId} not found`);
            }

            passEncoder.draw(vertexCount, instanceCount, firstVertex, firstInstance);
        } catch (error) {
            throw new Error(`Failed to draw: ${error.message}`);
        }
    }

    /**
     * Draw indexed vertices
     * @param {number} passEncoderId - Render pass encoder resource ID
     * @param {number} indexCount - Number of indices to draw
     * @param {number} instanceCount - Number of instances to draw
     * @param {number} firstIndex - First index
     * @param {number} baseVertex - Base vertex offset
     * @param {number} firstInstance - First instance index
     */
    drawIndexed(passEncoderId, indexCount, instanceCount, firstIndex, baseVertex, firstInstance) {
        try {
            const passEncoder = this.getResource(passEncoderId);
            if (!passEncoder) {
                throw new Error(`Render pass encoder with ID ${passEncoderId} not found`);
            }

            passEncoder.drawIndexed(indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
        } catch (error) {
            throw new Error(`Failed to draw indexed: ${error.message}`);
        }
    }

    /**
     * End a render pass
     * @param {number} passEncoderId - Render pass encoder resource ID
     */
    endRenderPass(passEncoderId) {
        try {
            const passEncoder = this.getResource(passEncoderId);
            if (!passEncoder) {
                throw new Error(`Render pass encoder with ID ${passEncoderId} not found`);
            }

            passEncoder.end();
            // Don't release the resource yet - it might be used for reference
        } catch (error) {
            throw new Error(`Failed to end render pass: ${error.message}`);
        }
    }

    /**
     * Finish command encoder and return command buffer
     * @param {number} encoderId - Command encoder resource ID
     * @returns {number} Resource ID for the command buffer
     */
    finishCommandEncoder(encoderId) {
        try {
            const encoder = this.getResource(encoderId);
            if (!encoder) {
                throw new Error(`Command encoder with ID ${encoderId} not found`);
            }

            const commandBuffer = encoder.finish();
            return this.storeResource(commandBuffer);
        } catch (error) {
            throw new Error(`Failed to finish command encoder: ${error.message}`);
        }
    }

    /**
     * Track and log WebGPU errors with deduplication
     * @param {string} message - Error message
     * @param {Error} error - The error object
     */
    trackError(message, error) {
        const errorKey = `${message}: ${error.message}`;
        const currentCount = this.errorCounts.get(errorKey) || 0;
        this.errorCounts.set(errorKey, currentCount + 1);

        // Limit the number of unique errors we track
        if (this.errorCounts.size > this.maxUniqueErrorsToTrack) {
            // Remove the oldest entry
            const firstKey = this.errorCounts.keys().next().value;
            this.errorCounts.delete(firstKey);
        }

        // Check if it's time to report
        const now = Date.now();
        if (now - this.lastErrorReport >= this.errorReportInterval) {
            this.reportErrorSummary();
            this.lastErrorReport = now;
        }

        // Only log the first occurrence of each error type to console
        if (currentCount === 0) {
            console.error(`${message}:`, error);
        }
    }

    /**
     * Report a summary of all tracked errors
     */
    reportErrorSummary() {
        if (this.errorCounts.size === 0) {
            return;
        }

        console.group(`WebGPU Error Summary (last ${this.errorReportInterval / 1000}s)`);
        
        // Sort by count (highest first)
        const sortedErrors = Array.from(this.errorCounts.entries())
            .sort((a, b) => b[1] - a[1]);

        let totalErrors = 0;
        sortedErrors.forEach(([errorMessage, count]) => {
            totalErrors += count;
            if (count > 1) {
                console.warn(`  ${count}x ${errorMessage}`);
            } else {
                console.warn(`  ${errorMessage}`);
            }
        });

        console.warn(`Total errors: ${totalErrors}`);
        console.groupEnd();

        // Clear the counts for the next interval
        this.errorCounts.clear();
    }

    /**
     * Get current error statistics
     * @returns {object} Error statistics including counts and summary
     */
    getErrorStatistics() {
        const stats = {
            uniqueErrors: this.errorCounts.size,
            totalErrors: 0,
            errors: []
        };

        this.errorCounts.forEach((count, message) => {
            stats.totalErrors += count;
            stats.errors.push({ message, count });
        });

        // Sort by count descending
        stats.errors.sort((a, b) => b.count - a.count);

        return stats;
    }

    /**
     * Clear all tracked errors
     */
    clearErrorStatistics() {
        this.errorCounts.clear();
        this.lastErrorReport = Date.now();
    }

    /**
     * Set the error reporting interval
     * @param {number} intervalMs - Interval in milliseconds
     */
    setErrorReportInterval(intervalMs) {
        this.errorReportInterval = Math.max(1000, intervalMs); // Min 1 second
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

export function createTextureView(textureId, descriptor) {
    return webGpuInterop.createTextureView(textureId, descriptor);
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

export function createBuffer(data, usage, label) {
    return webGpuInterop.createBuffer(data, usage, label);
}

export function writeBuffer(bufferId, data, offset) {
    webGpuInterop.writeBuffer(bufferId, data, offset);
}

export function createRenderPipeline(descriptor) {
    return webGpuInterop.createRenderPipeline(descriptor);
}

export function createBindGroup(descriptor) {
    return webGpuInterop.createBindGroup(descriptor);
}

export function createCommandEncoder(label) {
    return webGpuInterop.createCommandEncoder(label);
}

export function beginRenderPass(encoderId, descriptor) {
    return webGpuInterop.beginRenderPass(encoderId, descriptor);
}

export function setPipeline(passEncoderId, pipelineId) {
    webGpuInterop.setPipeline(passEncoderId, pipelineId);
}

export function setBindGroup(passEncoderId, index, bindGroupId) {
    webGpuInterop.setBindGroup(passEncoderId, index, bindGroupId);
}

export function setVertexBuffer(passEncoderId, slot, bufferId) {
    webGpuInterop.setVertexBuffer(passEncoderId, slot, bufferId);
}

export function setIndexBuffer(passEncoderId, bufferId, format) {
    webGpuInterop.setIndexBuffer(passEncoderId, bufferId, format);
}

export function draw(passEncoderId, vertexCount, instanceCount, firstVertex, firstInstance) {
    webGpuInterop.draw(passEncoderId, vertexCount, instanceCount, firstVertex, firstInstance);
}

export function drawIndexed(passEncoderId, indexCount, instanceCount, firstIndex, baseVertex, firstInstance) {
    webGpuInterop.drawIndexed(passEncoderId, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
}

export function endRenderPass(passEncoderId) {
    webGpuInterop.endRenderPass(passEncoderId);
}

export function finishCommandEncoder(encoderId) {
    return webGpuInterop.finishCommandEncoder(encoderId);
}

export function getErrorStatistics() {
    return webGpuInterop.getErrorStatistics();
}

export function clearErrorStatistics() {
    webGpuInterop.clearErrorStatistics();
}

export function setErrorReportInterval(intervalMs) {
    webGpuInterop.setErrorReportInterval(intervalMs);
}
