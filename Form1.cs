using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Zad4_TransmisjaSzeregowaRS232
{
    public partial class Form1 : Form
    {
        private List<string> profanityList;

        public Form1()
        {
            InitializeComponent();
            LoadProfanityList();
        }

        private void LoadProfanityList()         // laduje przeklenstwa do listy profanityList
        {
            var path = "C:\\Users\\Mati\\source\\repos\\OSK\\Zad4_TransmisjaSzeregowaRS232\\bin\\Debug\\profanityList.txt";
            profanityList = File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string>();
        }

        private string FilterProfanity(string input)           // zamienia przeklenstwa w pliku ProfanityList.txt na gwiazdki
        {
            foreach (var badWord in profanityList)
            {
                input = input.Replace(badWord, new string('*', badWord.Length));            
            }
            return input;
        }

        private string EncodeToRS232(string input)              
        {
            var sb = new StringBuilder();
            foreach (char c in input)
            {
                sb.Append("0"); // bit startu
                string bits = Convert.ToString(c, 2).PadLeft(8, '0');  // Convert.ToString(c, 2) zmienia znak na binarny kod ASCII, .PadLeft(8, '0') zapewnia ze zawsze sa 8 bitow
                sb.Append(new string(bits.Reverse().ToArray())); // odwraca bity (od LSB do MSB)
                sb.Append("11"); // dwa bity stopu
            }
            return sb.ToString();
        }

        private string DecodeFromRS232(string bitStream)
        {
            var result = new StringBuilder();
            for (int i = 0; i < bitStream.Length; i += 11)
            {
                if (i + 11 > bitStream.Length) break;     // 8 bitow znaku plus bit startu i bity stopu = 11
                string dataBits = bitStream.Substring(i + 1, 8); // pomija bit startu
                string normalBits = new string(dataBits.Reverse().ToArray());  // odwraca bity spowrotem od MSB do LSB
                char c = (char)Convert.ToInt32(normalBits, 2); // zmieniamy na znak ASCII pomijajac dwa pierwsze bity, czyli w tym przypadku bity stopu
                result.Append(c);
            }
            return result.ToString();
        }

        private void Send_Click_1(object sender, EventArgs e)
        {
            string inputText = FilterProfanity(txtInput.Text);   // sprawdza obecnosc przeklenstw
            string encoded = EncodeToRS232(inputText);  // enkoduje
            txtEncoded.Text = encoded;
            
            File.WriteAllText("transmission.txt", encoded);  // zapisuje zakodowana transmisje do pliku txt
        }

        private void Receive_Click_1(object sender, EventArgs e)
        {
            if (!File.Exists("transmission.txt")) return;
            string receivedBits = File.ReadAllText("transmission.txt");    // odczytuje transmisje z pliku
            string decoded = DecodeFromRS232(receivedBits);  // dekoduje
            txtOutput.Text = FilterProfanity(decoded);
        }
    }
}
