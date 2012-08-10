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
        /// ����ֵ
        /// </param>
        /// <param name="value">
        /// ʶ����
        /// </param>
        public RecognizeSample(string eigenValue, string value)
        {
            this.EigenValue = eigenValue;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets ��������ֵ
        /// </summary>
        public string EigenValue { get; set; }

        /// <summary>
        /// Gets or sets ֵ
        /// </summary>
        public string Value { get; set; }
    }
}