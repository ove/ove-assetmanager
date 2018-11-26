using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OVE.Service.Core.Assets;

namespace OVE.Service.Core.FileOperations {
    /// <summary>
    /// An interface for dealing with the actual file of an Asset
    /// </summary>
    public interface IAssetFileOperations {
        string ResolveFileUrl(OVEAssetModel asset);
        // ReSharper disable twice UnusedParameter.Global
        Task<bool> Move(OVEAssetModel oldAsset, OVEAssetModel newAsset);
        Task<bool> Delete(OVEAssetModel asset);
        Task<bool> Save(OVEAssetModel asset, IFormFile upload);
        Task Upload(string bucketName, string assetStorageLocation, Stream file);
        Task<bool> UploadDirectory(string file, OVEAssetModel asset);
        /// <summary>
        /// Sanitize the file extension
        /// </summary>
        /// <param name="input">the fil extension</param>
        /// <returns>sanitized version</returns>
        string SanitizeExtension(string input);
        /// <summary>
        /// Validate a file name
        /// </summary>
        /// <param name="input">file name without extension</param>
        /// <param name="extension">the extension, which should be validated </param>
        /// <returns>a sanitized version of the file name</returns>
        string SanitizeFilename(string input, string extension);
    }
}