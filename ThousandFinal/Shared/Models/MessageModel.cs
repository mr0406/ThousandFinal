using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Models
{
    public class MessageModel
    {
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public bool GeneratedFromServer { get; set; } = false;

        public MessageModel() { }

        public MessageModel(string text, UserModel author)
        {
            Text = text;
            AuthorName = author.Name;
        }

        public MessageModel(string text, bool generatedFromServer)
        {
            Text = text;
            GeneratedFromServer = generatedFromServer;
        }
    }
}
