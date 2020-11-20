using System;

namespace UnityEngine.Rendering.HighDefinition
{
    /// <summary>
    /// Context used when executing custom passes
    /// </summary>
    public struct CustomPassContext
    {
        /// <summary>
        /// Scriptable Render Context, used for any SRP related operations.
        /// </summary>
        public readonly ScriptableRenderContext  renderContext;

        /// <summary>
        /// Command Buffer, used to enqueue graphic commands to the GPU.
        /// </summary>
        public readonly CommandBuffer            cmd;

        /// <summary>
        /// HdCamera, HDRP data related to the rendering camera. Use the camera property to access the Camera class.
        /// </summary>
        public readonly HDCamera                 hdCamera;

        /// <summary>
        /// Result of the culling either of the camera or the custom pass if AggregateCullingParameters is used.
        /// </summary>
        public readonly CullingResults           cullingResults;

        /// <summary>
        /// Camera color buffer.
        /// </summary>
        public readonly RTHandle                 cameraColorBuffer;

        /// <summary>
        /// Camera depth buffer.
        /// </summary>
        public readonly RTHandle                 cameraDepthBuffer;

        /// <summary>
        /// Camera normal buffer.
        /// </summary>
        public readonly RTHandle                 cameraNormalBuffer;

        /// <summary>
        /// Lazy handle to the custom color buffer, not allocated if not used.
        /// </summary>
        public readonly Lazy<RTHandle>           customColorBuffer;

        /// <summary>
        /// Lazy handle to the custom depth buffer, not allocated if not used.
        /// </summary>
        public readonly Lazy<RTHandle>           customDepthBuffer;

        /// <summary>
        /// Material Property Block, unique for each custom pass instance.
        /// </summary>
        public readonly MaterialPropertyBlock    propertyBlock;

        internal CustomPassContext(
            ScriptableRenderContext renderContext, CommandBuffer cmd,
            HDCamera hdCamera, CullingResults cullingResults,
            RTHandle cameraColorBuffer, RTHandle cameraDepthBuffer,
            RTHandle cameraNormalBuffer, Lazy<RTHandle> customColorBuffer,
            Lazy<RTHandle> customDepthBuffer, MaterialPropertyBlock propertyBlock)
        {
            this.renderContext = renderContext;
            this.cmd = cmd;
            this.hdCamera = hdCamera;
            this.cullingResults = cullingResults;
            this.cameraColorBuffer = cameraColorBuffer;
            this.cameraDepthBuffer = cameraDepthBuffer;
            this.customColorBuffer = customColorBuffer;
            this.cameraNormalBuffer = cameraNormalBuffer;
            this.customDepthBuffer = customDepthBuffer;
            this.propertyBlock = propertyBlock;
        }
    }
}
