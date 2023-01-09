﻿using System.Net.Http.Headers;
using System.Xml.Linq;
using WebDav.Client.Core;

namespace WebDav
{
    /// <summary>
    /// Represents parameters for the PROPFIND WebDAV method.
    /// </summary>
    public class PropfindParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropfindParameters"/> class.
        /// </summary>
        public PropfindParameters()
        {
            RequestType = PropfindRequestType.AllProperties;
            CustomProperties = new List<XName>();
            Namespaces = new List<NamespaceAttr>();
            ContentType = MediaTypes.XmlMediaType;
            Headers = new List<KeyValuePair<string, string>>();
            CancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// Gets or sets a type of PROPFIND request.
        /// AllProperties: 'allprop' + 'include'.
        /// NamedProperties: 'prop'.
        /// </summary>
        public PropfindRequestType RequestType { get; set; }

        /// <summary>
        /// Gets or sets the collection of custom properties (or dead properties in terms of WebDav).
        /// </summary>
        public IReadOnlyCollection<XName> CustomProperties { get; set; }

        /// <summary>
        /// Gets or sets the collection of xml namespaces of properties.
        /// </summary>
        public IReadOnlyCollection<NamespaceAttr> Namespaces { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the method is to be applied only to the resource, to the resource and its internal members only, or the resource and all its members.
        /// It corresponds to the WebDAV Depth header.
        /// </summary>
        public ApplyTo.Propfind? ApplyTo { get; set; }

        /// <summary>
        /// Gets or sets the content type of the request body.
        /// The default value is application/xml; charset=utf-8.
        /// </summary>
        public MediaTypeHeaderValue ContentType { get; set; }

        /// <summary>
        /// Gets or sets the collection of http request headers.
        /// </summary>
        public IReadOnlyCollection<KeyValuePair<string, string>> Headers { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }
}
