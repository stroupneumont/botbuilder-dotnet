﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Contains recognition results generated by an <see cref="IRecognizer"/>.
    /// </summary>
    /// <seealso cref="IRecognizer.RecognizeAsync(string, System.Threading.CancellationToken)"/>
    public class RecognizerResult : IRecognizerConvert
    {
        /// <summary>
        /// Gets or sets the input text to recognize.
        /// </summary>
        /// <value>The input text.</value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the input text as modified by the recognizer, for example for spelling correction.
        /// </summary>
        /// <value>The modified input text.</value>
        [JsonProperty("alteredText")]
        public string AlteredText { get; set; }

        /// <summary>
        /// Gets or sets the recognized intents, with the intent as key and the confidence as value.
        /// </summary>
        /// <value>The recognized intents.</value>
        [JsonProperty("intents")]
        public JObject Intents { get; set; }

        /// <summary>
        /// Gets or sets the recognized top-level entities.
        /// </summary>
        /// <value>The recognized top-level entities.</value>
        [JsonProperty("entities")]
        public JObject Entities { get; set; }

        /// <summary>
        /// Gets or sets any extra properties the recognizer includes in the results.
        /// </summary>
        /// <value>Extra information returned by the recognizer.</value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public void Convert(dynamic result)
        {
            Text = result.Text;
            AlteredText = result.AlteredText;
            Intents = result.Intents;
            Entities = result.Entities;
            Properties = result.Properties;
        }
    }
}
