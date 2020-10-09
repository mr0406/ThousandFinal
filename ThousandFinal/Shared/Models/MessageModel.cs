namespace ThousandFinal.Shared.Models
{
    public class MessageModel
    {
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public bool GeneratedFromServer { get; set; } = false;

        public MessageModel() { }

        public MessageModel(string text, string authorName)
        {
            Text = text;
            AuthorName = authorName;
        }

        public MessageModel(string text, bool generatedFromServer)
        {
            Text = text;
            GeneratedFromServer = generatedFromServer;
        }
    }
}
