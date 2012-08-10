namespace ValidateCodeRecognize.Core
{
    /// <summary>
    /// </summary>
    public class RecognizeSample
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecognizeSample"/> class. 
        /// </summary>
        /// <param name="eigenValue">
        /// 特征值
        /// </param>
        /// <param name="value">
        /// 识别结果
        /// </param>
        public RecognizeSample(string eigenValue, string value)
        {
            this.EigenValue = eigenValue;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets 样本特征值
        /// </summary>
        public string EigenValue { get; set; }

        /// <summary>
        /// Gets or sets 值
        /// </summary>
        public string Value { get; set; }
    }
}