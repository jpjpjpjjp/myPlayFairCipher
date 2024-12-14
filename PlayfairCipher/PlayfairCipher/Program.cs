using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlayfairCipher
{
    public partial class MainForm : Form
    {
        private readonly char[,] playfairMatrix = new char[5, 5];
        private readonly TextBox txtKeyword;
        private readonly TextBox txtPlainText;
        private readonly TextBox txtEncryptedText;
        private readonly Label lblDecryptedText;
        private readonly Button btnEncrypt;
        private readonly Button btnDecrypt;
        private readonly Button btnClear;
        private readonly Button btnExit;

        public MainForm()
        {
            txtKeyword = new TextBox { PlaceholderText = "Enter keyword" };
            txtPlainText = new TextBox { PlaceholderText = "Enter plain text" };
            txtEncryptedText = new TextBox { ReadOnly = true };
            lblDecryptedText = new Label { Text = "Decrypted Text:" };
            btnEncrypt = new Button { Text = "Encrypt" };
            btnDecrypt = new Button { Text = "Decrypt" };
            btnClear = new Button { Text = "Clear" };
            btnExit = new Button { Text = "Exit" };

            LoadControls();
        }

        private void LoadControls()
        {
            btnEncrypt.Click += BtnEncrypt_Click;
            btnDecrypt.Click += BtnDecrypt_Click;
            btnClear.Click += BtnClear_Click;
            btnExit.Click += BtnExit_Click;

            Controls.Add(txtKeyword);
            Controls.Add(txtPlainText);
            Controls.Add(txtEncryptedText);
            Controls.Add(lblDecryptedText);
            Controls.Add(btnEncrypt);
            Controls.Add(btnDecrypt);
            Controls.Add(btnClear);
            Controls.Add(btnExit);

            txtKeyword.SetBounds(20, 20, 200, 30);
            txtPlainText.SetBounds(20, 60, 200, 30);
            btnEncrypt.SetBounds(20, 100, 90, 30);
            btnDecrypt.SetBounds(120, 100, 90, 30);
            txtEncryptedText.SetBounds(20, 140, 200, 30);
            lblDecryptedText.SetBounds(20, 180, 200, 30);
            btnClear.SetBounds(20, 220, 90, 30);
            btnExit.SetBounds(120, 220, 90, 30);
        }

        private async void BtnEncrypt_Click(object? sender, EventArgs e)
        {
            try
            {
                var keyword = txtKeyword.Text.ToUpper();
                var plainText = txtPlainText.Text.ToLower();

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    MessageBox.Show("Please enter a keyword.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(plainText))
                {
                    MessageBox.Show("Please enter plain text to encrypt.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var encryptedText = await Task.Run(() => Encrypt(plainText, keyword));

                txtEncryptedText.Text = encryptedText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during encryption: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDecrypt_Click(object? sender, EventArgs e)
        {
            try
            {
                var keyword = txtKeyword.Text.ToUpper();
                var encryptedText = txtEncryptedText.Text.ToUpper();

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    MessageBox.Show("Please enter a keyword.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(encryptedText))
                {
                    MessageBox.Show("Please enter encrypted text to decrypt.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var decryptedText = await Task.Run(() => Decrypt(encryptedText, keyword));

                lblDecryptedText.Text = $"Decrypted Text: {decryptedText}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during decryption: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            txtKeyword.Clear();
            txtPlainText.Clear();
            txtEncryptedText.Clear();
            lblDecryptedText.Text = "Decrypted Text:";
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoadPlayfairMatrix(string keyword)
        {
            var usedChars = new bool[26];
            int k = 0;

            keyword = keyword.Replace("J", "I");

            foreach (char c in keyword)
            {
                if (char.IsLetter(c))
                {
                    int index = c - 'A';
                    if (!usedChars[index])
                    {
                        playfairMatrix[k / 5, k % 5] = c;
                        usedChars[index] = true;
                        k++;
                    }
                }
            }

            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (c == 'J') continue; 

                int index = c - 'A';
                if (!usedChars[index])
                {
                    playfairMatrix[k / 5, k % 5] = c;
                    usedChars[index] = true;
                    k++;
                }
            }
        }

        private string Encrypt(string plainText, string keyword)
        {
            LoadPlayfairMatrix(keyword);

            plainText = plainText.Replace("j", "i").Replace(" ", "").ToLower();

            StringBuilder processedText = new StringBuilder();

            
            int i = 0;
            while (i < plainText.Length)
            {
                char a = plainText[i];
                char b = '\0';

                if (i + 1 < plainText.Length)
                {
                    b = plainText[i + 1];
                    if (a == b)
                    {
                        b = 'x';
                        i++;
                    }
                    else
                    {
                        i += 2;
                    }
                }
                else
                {
                    b = 'x';
                    i++;
                }

                processedText.Append(a);
                processedText.Append(b);
            }

            StringBuilder encrypted = new StringBuilder();

            for (i = 0; i < processedText.Length; i += 2)
            {
                char a = processedText[i];
                char b = processedText[i + 1];

                var (rowA, colA) = FindPosition(char.ToUpper(a));
                var (rowB, colB) = FindPosition(char.ToUpper(b));

                if (rowA == rowB)
                {
                    encrypted.Append(playfairMatrix[rowA, (colA + 1) % 5]);
                    encrypted.Append(playfairMatrix[rowB, (colB + 1) % 5]);
                }
                else if (colA == colB)
                {
                    encrypted.Append(playfairMatrix[(rowA + 1) % 5, colA]);
                    encrypted.Append(playfairMatrix[(rowB + 1) % 5, colB]);
                }
                else
                {
                    encrypted.Append(playfairMatrix[rowA, colB]);
                    encrypted.Append(playfairMatrix[rowB, colA]);
                }
            }

            return encrypted.ToString();
        }

        private string Decrypt(string encryptedText, string keyword)
        {
            LoadPlayfairMatrix(keyword);

            encryptedText = encryptedText.Replace(" ", "").ToUpper();

            if (encryptedText.Length % 2 != 0)
            {
                throw new ArgumentException("Encrypted text length must be even.");
            }

            StringBuilder decrypted = new StringBuilder();

            for (int i = 0; i < encryptedText.Length; i += 2)
            {
                char a = encryptedText[i];
                char b = encryptedText[i + 1];

                var (rowA, colA) = FindPosition(a);
                var (rowB, colB) = FindPosition(b);

                if (rowA == rowB)
                {
                    decrypted.Append(playfairMatrix[rowA, (colA + 4) % 5]);
                    decrypted.Append(playfairMatrix[rowB, (colB + 4) % 5]);
                }
                else if (colA == colB)
                {
                    decrypted.Append(playfairMatrix[(rowA + 4) % 5, colA]);
                    decrypted.Append(playfairMatrix[(rowB + 4) % 5, colB]);
                }
                else
                {
                    decrypted.Append(playfairMatrix[rowA, colB]);
                    decrypted.Append(playfairMatrix[rowB, colA]);
                }
            }


            string result = decrypted.ToString().Replace("X", "").ToLower();

            return result;
        }
        //I think This was causing the infinite loop i pray to God its fixed
        private (int row, int col) FindPosition(char c)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (playfairMatrix[i, j] == c)
                        return (i, j);
                }
            }
            throw new ArgumentException($"Character '{c}' not found in Playfair matrix.");
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}


