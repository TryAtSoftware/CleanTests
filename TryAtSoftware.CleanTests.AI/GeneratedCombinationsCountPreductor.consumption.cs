﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
namespace TryAtSoftware_CleanTests_AI
{
    public partial class GeneratedCombinationsCountPreductor
    {
        /// <summary>
        /// model input class for GeneratedCombinationsCountPreductor.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [ColumnName(@"Categories")]
            public string Categories { get; set; }

            [ColumnName(@"IncompatiblePairs")]
            public string IncompatiblePairs { get; set; }

            [ColumnName(@"Count")]
            public float Count { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for GeneratedCombinationsCountPreductor.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"Categories")]
            public float[] Categories { get; set; }

            [ColumnName(@"IncompatiblePairs")]
            public float[] IncompatiblePairs { get; set; }

            [ColumnName(@"Count")]
            public float Count { get; set; }

            [ColumnName(@"Features")]
            public float[] Features { get; set; }

            [ColumnName(@"Score")]
            public float Score { get; set; }

        }

        #endregion

        private static string MLNetModelPath = Path.GetFullPath("GeneratedCombinationsCountPreductor.zip");

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }
    }
}
