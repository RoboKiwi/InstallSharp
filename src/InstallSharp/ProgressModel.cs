namespace InstallSharp
{
    public class ProgressModel
    {
        public ProgressModel(ProgressState state, string caption, int value)
        {
            State = state;
            Caption = caption;
            Value = value;
        }

        public ProgressState State { get; set; }
        public string Caption { get; set; }
        public int Value { get; set; }
    }
}