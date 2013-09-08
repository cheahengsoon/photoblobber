var azure = require('azure'); // for accessing Storage service
var qs = require('querystring'); // for parsing and manipulating query strings

function insert(item, user, request) {
    var accountName = ''; // add your storage account name
    var accountKey = ''; // add your storage account key
    var host = accountName + '.blob.core.windows.net';
    var canonicalizedResource = '/' + item.ContainerName + '/' + item.ResourceName;
    item.ContainerName = item.ContainerName.toLowerCase();    // must be lowercase

    // create the container if it does not exist
    // we will use public read access for the blobs and will use a SAS to upload 
    var blobService = azure.createBlobService(accountName, accountKey, host);
    blobService.createContainerIfNotExists(
        item.ContainerName, { publicAccessLevel: 'blob' },
         function (error) {
             if (!error) {
                 // container exists now define a policy that provides write access 
                 // that starts immediately and expires in 5 mins 
                 var sharedAccessPolicy = {
                     AccessPolicy: {
                         Permissions: azure.Constants.BlobConstants.SharedAccessPermissions.WRITE,
                         Expiry: formatDate(new Date(new Date().getTime() + 5 * 60 * 1000)) //5 minutes from now
                     }
                 };

                 // generate the SAS for your BLOB
                 var sasQueryString = getSAS(accountName, accountKey, canonicalizedResource, azure.Constants.BlobConstants.ResourceTypes.BLOB, sharedAccessPolicy);

                 // Store blob URL and SAS
                 item.BlobUrl = 'https://' + host + canonicalizedResource;
                 item.SAS = sasQueryString;

             } else { console.error(error); }

             request.execute();
         });
}

function getSAS(accountName, accountKey, path, resourceType, sharedAccessPolicy) {
    return qs.encode(
        new azure.SharedAccessSignature(accountName, accountKey).generateSignedQueryString(path, {}, resourceType, sharedAccessPolicy));
}

function formatDate(date) {
    var raw = date.toJSON();
    //blob service does not like milliseconds on the end of the time so strip
    return raw.substr(0, raw.lastIndexOf('.')) + 'Z';
}
