using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Threading;
using System.IO;
using System.Globalization;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class LineHexAddress : RichTextBox
    {
        private bool state = true;
        private bool enabled = true;
        private int[] mein;

        public void Initialize()
        {
            mein = new int[4096];

            for (int i = 0; i < 4096; i++)
            {
                mein[i] = -100;
            }
        }

        public void Delete(int size)
        {
            if (size < this.Document.Blocks.Count)
            {
                TextPointer starto = this.Document.Blocks.ElementAt(size).ElementStart;
                TextRange tempo = new TextRange(starto, this.Document.ContentEnd);
                Dispatcher.Invoke(new System.Action(() => tempo.Text = ""));
            }
        }

        public void Refresh2(int[] address, int size)
        {
            if (!enabled)
            {
                return;
            }

            TextRange edit = null;
            Dispatcher.Invoke(new System.Action(() => edit = new TextRange(this.Document.ContentStart, this.Document.ContentEnd)));
            string temp_address = "";

            for (int i = 0; i < size; i++)
            {
                if (address[i] == -10)
                {
                    Dispatcher.Invoke(new System.Action(() => edit.Text = temp_address));
                    return;
                }
                else if (address[i] == -1)
                {
                    temp_address += ((i != 0) ? "\n" : "") + "---";
                }
                else if (address[i] == -2)
                {
                    temp_address += ((i != 0) ? "\n" : "") + "-->";
                }
                else if (address[i] == -3)
                {
                    temp_address += ((i != 0) ? "\n" : "") + "<--";
                }
                else
                {
                    temp_address += ((i != 0) ? "\n" : "") + address[i].ToString(state ? "X3" : "D4");
                }
            }

            Dispatcher.Invoke(new System.Action(() => edit.Text = temp_address));
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            state = !state;
            base.OnMouseDoubleClick(e);
        }
    }

    public class ParkamList : ListBox
    {
        public void Update(ArrayList arg)
        {
            ArrayList Whole = new ArrayList();
            ArrayList toDelete = new ArrayList();
            ArrayList toAdd = new ArrayList();

            foreach (string item in arg)
            {
                if (!this.Items.Contains(item))
                {
                    toAdd.Add(item);
                }
            }

            foreach (string item in this.Items)
            {
                if (!arg.Contains(item))
                {
                    toDelete.Add(item);
                }
            }

            foreach (string item in toDelete)
            {
                Dispatcher.Invoke(new System.Action(() => this.Items.Remove(item)));
            }

            foreach (string item in toAdd)
            {
                Dispatcher.Invoke(new System.Action(() => this.Items.Add(item)));
            }
        }

        public void Update(ArrayList arg, ArrayList arg2)
        {
            ArrayList Whole = new ArrayList();
            ArrayList toDelete = new ArrayList();
            ArrayList toAdd = new ArrayList();

            foreach (string item in arg)
            {
                if (!this.Items.Contains(item))
                {
                    toAdd.Add(item);
                }
            }

            foreach (string item in arg2)
            {
                if (!this.Items.Contains(item))
                {
                    toAdd.Add(item);
                }
            }

            foreach (string item in this.Items)
            {
                if (!arg.Contains(item) & !arg2.Contains(item))
                {
                    toDelete.Add(item);
                }
            }

            foreach (string item in toDelete)
            {
                Dispatcher.Invoke(new System.Action(() => this.Items.Remove(item)));
            }

            foreach (string item in toAdd)
            {
                Dispatcher.Invoke(new System.Action(() => this.Items.Add(item)));
            }
        }
    }

    public class ParkamCodeBox : RichTextBox
    {
        private char[] trim  = { ' ', '\t' };
        private char[] trim2 = { '\n' };
        private char[] trim3 = { '\n', '\r' };
        private char[] trim4 = { ' ', '\t', '\n', '\r' };

        public  bool   IsSaved                     = false;
        public  bool   IsCreated                   = false;

        public  Setting studio_setting = new Setting();

        private int    ExNumber            = 0;
        private byte[] hexa = new byte[4096];
        private int [] UsedAddress = new int [4096];

        private string Directory;
        private string HexFileContent;
        private string ParkamHexFileContent;
        //---------------------------------------------
        private Thread thread_load;
        private Thread thread_check;
        private Thread thread_timer;
        private Thread thread_assemble;
        private Thread thread_analyze;
        //-------------------------------------------->Reference to controls
        private ListBox         AutoSense;
        private TextBlock       Status;
        private ProgressBar     StatusBar;

        private ParkamList      ListError;
        private ParkamList      ListMessage;
        private ParkamList      ListWarning;

        private TextBox         Memory;
        private TextBox         Other;
        private TextBox         AnalyzerResult;

        private RichTextBox     Result;
        private RichTextBox     LineNumber;

        private LineHexAddress  LineHelper;
        //------------------------------------------
        private ArrayList PMOVs                = new ArrayList();
        private ArrayList PMVIs                = new ArrayList();
        private ArrayList JUMPs                = new ArrayList();
        private ArrayList Labels               = new ArrayList();
        private ArrayList Errors               = new ArrayList();
        private ArrayList Warning              = new ArrayList();
        private ArrayList MemoryContent        = new ArrayList();
        private ArrayList One_Byte_Instruction = new ArrayList();

        PrintDialog PrintFile = new PrintDialog();
        Microsoft.Win32.SaveFileDialog SaveFile = new Microsoft.Win32.SaveFileDialog();
        Microsoft.Win32.OpenFileDialog OpenFile = new Microsoft.Win32.OpenFileDialog();

        //---------------------------------------------------------------------------->

        public  void Initialize(ref LineHexAddress LineHelper, ref RichTextBox LineNumber, ref ParkamList ListError, ref ParkamList ListWarning, ref ParkamList ListMessage, ref ListBox AutoSense, ref ProgressBar StatusBar, ref TextBlock Status, ref TextBox Memory, ref TextBox Other , ref RichTextBox Result , ref TextBox AnalyzerResult)
        {
            this.Status = Status;
            this.Result = Result;
            this.Other = Other;
            this.Memory = Memory;
            this.AutoSense = AutoSense;
            this.StatusBar = StatusBar;
            this.LineNumber = LineNumber;
            this.LineHelper = LineHelper;
            this.ListError = ListError;
            this.ListWarning = ListWarning;
            this.ListMessage = ListMessage;
            this.AnalyzerResult = AnalyzerResult;

            thread_check = new Thread(Check);
            thread_timer = new Thread(AutoCompleteTimer);
            thread_check.Start();

            //LineNumber.AppendText("1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12\n13\n14\n1\n15\n16\n17\n18\n19\n20");

            PMVIs.Add("A,");
            PMVIs.Add("X,");

            PMOVs.Add("A,B");
            PMOVs.Add("B,A");
            PMOVs.Add("A,X");
            PMOVs.Add("X,A");

            JUMPs.Add("JMP");
            JUMPs.Add("JZR");
            JUMPs.Add("JNZ");
            JUMPs.Add("JCY");
            JUMPs.Add("JNC");
            JUMPs.Add("JNG");
            JUMPs.Add("JPS");
            JUMPs.Add("JOV");

            One_Byte_Instruction.Add("ADD");
            One_Byte_Instruction.Add("SUB");
            One_Byte_Instruction.Add("CMP");
            One_Byte_Instruction.Add("DEC");
            One_Byte_Instruction.Add("AND");
            One_Byte_Instruction.Add("OR");
            One_Byte_Instruction.Add("XOR");
            One_Byte_Instruction.Add("CPTA");
            One_Byte_Instruction.Add("RORA");
            One_Byte_Instruction.Add("ROLA");
            One_Byte_Instruction.Add("RORC");
            One_Byte_Instruction.Add("ROLC");
            One_Byte_Instruction.Add("CLC");
            One_Byte_Instruction.Add("CPTC");
            One_Byte_Instruction.Add("INC");
            One_Byte_Instruction.Add("HLT");
            One_Byte_Instruction.Add("NOOP");
            One_Byte_Instruction.Add("SWP");
            One_Byte_Instruction.Add("INA");
            One_Byte_Instruction.Add("OUTA");

            UsedAddress[0] = 0;

            for (int i = 1; i < 4096; i++)
            {
                UsedAddress[i] = -10;
            }

            //SaveFile.CheckFileExists = true;
            SaveFile.CheckPathExists = true;
            SaveFile.AddExtension = true;
            SaveFile.RestoreDirectory = true;
            SaveFile.OverwritePrompt = true;

            OpenFile.CheckFileExists = true;
            OpenFile.CheckPathExists = true;
            OpenFile.AddExtension = true;
            OpenFile.RestoreDirectory = true;
            OpenFile.Multiselect = false;
            OpenFile.Filter = "Parkam Files (*.pkm)|*.pkm|Parkam Hex Files (*.pkmhex)|*.pkmhex|Parkam Lst Files (*.pkmlst)|*.pkmlst|Assembly Files (*.asm)|*.asm|Text Files (*.txt)|*.txt|Hex Files (*.hex)|*.hex|Lst Files (*.lst)|*.lst|Result Files (*.res)|*.res";
        }

        private int  Validate_Address(string Address, int LineCounter)
        {
            int parsed_value = 0;

            switch (Address[Address.Length - 1])
            {
                case 'B'://binary address inserted
                    {
                        if (Address.Length != 13)
                        {
                            Errors.Add("Error In Line " + LineCounter.ToString() + " : Address Length is not correct ( It must consist of 12 bits ) ");
                            return -1;
                        }

                        foreach (char letter in Address.Substring(0, Address.Length - 1))
                        {
                            if (letter != '0' & letter != '1')
                            {
                                Errors.Add("Error In Line " + LineCounter.ToString() + " : Address is not in binary format ");
                                return -1;
                            }
                        }

                        return Convert.ToInt32(Address.Substring(0, Address.Length - 1), 2) + 2000;

                        break;
                    }

                case 'H'://hexadecimal address inserted
                    {
                        if (Address.Length != 4)
                        {
                            Errors.Add("Error In Line " + LineCounter.ToString() + " : Address Length is not correct ( It must consist of 3 hexadecimal digits ) ");
                            return -1;
                        }
                        else if (!Int32.TryParse(Address.Substring(0, 3), System.Globalization.NumberStyles.AllowHexSpecifier, null, out parsed_value))
                        {
                            Errors.Add("Error In Line " + LineCounter.ToString() + " : Address is not in hexadecimal format ");
                            return -1;
                        }

                        return parsed_value + 2000;

                        break;
                    }

                default://inserted address is decimal
                    {
                        if (Int32.TryParse(Address, out parsed_value))
                        {
                            if (parsed_value > 4095 | parsed_value < 0)
                            {
                                Errors.Add("Error In Line " + LineCounter.ToString() + " : Address is out of range ( It must be between 0 and 4095 ) ");
                                return -1;
                            }

                            return parsed_value + 2000;
                        }
                        else
                        {
                            Errors.Add("Error In Line " + LineCounter.ToString() + " : Address is not recognized ");
                            return -1;
                        }

                        break;
                    }
            }

            return -1;
        }

        private int  Analyze_Line2(string line, string text, int Line_Number, string[] lines)
        {
            Queue words = new Queue();

            string[] line_words = line.Split(trim, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < line_words.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(line_words[i]))
                {
                    char temp = line_words[i][line_words[i].Length - 1];

                    if (temp == '\n' | temp == '\r' | temp == ' ')
                    {
                        line_words[i] = line_words[i].Substring(0, line_words[i].Length - 1);

                        if (!String.IsNullOrWhiteSpace(line_words[i]))
                        {
                            words.Enqueue(line_words[i]);
                        }
                    }
                    else
                    {
                        words.Enqueue(line_words[i]);
                    }
                }
            }

            if (words.Count == 0)
            {
                return -1;
            }

            string temp_first_word = (string)words.Dequeue();
            string first_word = temp_first_word.ToUpperInvariant();

            if (first_word[first_word.Length - 1] == ':')
            {
                if (!IsThere(temp_first_word, Line_Number - 1, lines))
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : This label is used before ");
                    return -1;
                }

                temp_first_word = temp_first_word.Substring(0, temp_first_word.Length - 1);

                if (!Labels.Contains(temp_first_word))
                {
                    Labels.Add(temp_first_word);
                }

                if (words.Count == 0)
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Label can't point to empty location");
                    return -1;
                }

                first_word = ((string)words.Dequeue()).ToUpperInvariant();

            }

            if (first_word == ".ORG")
            {
                if (words.Count == 0)
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Expected address after .org pseudo instruction ");
                    return -1;
                }
                else if (words.Count > 1)
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Unrecognized word(s) after .org address ( " + words.Peek() + " )");
                    return -1;
                }
                else
                {
                    int temp = Validate_Address((string)words.Dequeue(), Line_Number);

                    if (temp == -1)
                    {
                        Errors.Add("Error in Line " + Line_Number.ToString() + " : Unrecognized address after .org pseudo instruction ");
                        return -1;
                    }
                    else
                    {
                        return temp;
                    }
                }
            }

            if (first_word == ".END")
            {
                return -3;
            }

            if (One_Byte_Instruction.Contains(first_word))
            {
                if (words.Count != 0)
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Unexpected word after one byte instruction ( " + words.Dequeue() + " ) ");
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else if (first_word == "PMOV")
            {
                if (words.Count == 0)
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Expected register names after Pmov instruction ");
                    return -1;
                }
                else if (words.Count > 1)
                {
                    words.Dequeue();
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Unexpected word(s) after Pmov instruction ( " + words.Peek() + " )");
                    return -1;
                }
                else if (!PMOVs.Contains(((string)words.Dequeue()).ToUpperInvariant()))
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Unrecognized register names after Pmov instruction ");
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else if (first_word == "PMVI")
            {
                if (words.Count == 0)
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Expected register name after Pmov instruction ");
                    return -1;
                }
                else if (words.Count > 2)
                {
                    words.Dequeue();
                    words.Dequeue();
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Unexpected word(s) after Pmvi instruction ( " + words.Peek() + " )");
                    return -1;
                }
                else if (!PMVIs.Contains(((string)words.Dequeue()).ToUpperInvariant()))
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Unrecognized register name after PMVI instruction ");
                    return -1;
                }
                else
                {
                    if (words.Count == 0)
                    {
                        Errors.Add("Error in Line " + Line_Number.ToString() + " : Expected data after register name ");
                        return -1;
                    }
                    else if (!Validate_Data(((string)words.Dequeue()).ToUpperInvariant(), Line_Number))
                    {
                        Errors.Add("Error in Line " + Line_Number.ToString() + " : Unrecognized data after PMVI instruction ");
                        return -1;
                    }
                    else
                    {
                        return 2;
                    }
                }
            }
            else if (first_word == "LDA" | first_word == "STA")
            {
                if (words.Count == 0)
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Expected address after " + first_word);
                    return -1;
                }
                else if (words.Count > 1)
                {
                    words.Dequeue();
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Unexpected word(s) after " + first_word[0] + first_word.Substring(1).ToLowerInvariant() + " ( " + words.Peek() + " )");
                    return -1;
                }
                else
                {
                    if (-1 == Validate_Address(((string)words.Dequeue()).ToUpperInvariant(), Line_Number))
                    {
                        Errors.Add("Error in Line " + Line_Number.ToString() + " : Unrecognized address after " + first_word);
                        return -1;
                    }
                    else
                    {
                        return 2;
                    }
                }

            }
            else if (JUMPs.Contains(first_word))
            {
                if (words.Count == 0)
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Expected label after Jump instruction ");
                    return -1;
                }
                else if (words.Count > 1)
                {
                    words.Dequeue();
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Unexpected word(s) after Jump instruction ( " + words.Peek() + " )");
                    return -1;
                }

                string temp3 = (string)words.Dequeue();

                if (temp3 == "$")
                {
                    return 2;
                }
                if (!Labels.Contains(temp3))
                {
                    Errors.Add("Error in Line " + Line_Number.ToString() + " : Undefined label ( " + temp3 + " ) ");
                    return -1;
                }

                return 2;
            }
            else
            {
                Errors.Add("Error in Line " + Line_Number.ToString() + " : Unrecognized word ( " + temp_first_word + " ) ");
                return -1;
            }
        }

        private bool Validate_Data(string data, int LineCounter)
        {
            int parsed_value = 0;

            if (data.Length >= 5)
            {
                if (data.Substring(0, 4) == "LOW(" & data[data.Length - 1] == ')')
                {
                    if (data.Length != 10)
                    {
                        Errors.Add("Error In Line " + LineCounter.ToString() + " : Data Length is not correct ( It must consist of 5 decimal number ) ");
                        return false;
                    }

                    if (!Int32.TryParse(data.Substring(4, 5), out parsed_value))
                    {
                        Errors.Add("Error In Line " + LineCounter.ToString() + " : Value between parenthysis is not recognized ");
                        return false;
                    }
                    else if (parsed_value <= -32768 | 32767 <= parsed_value)
                    {
                        Errors.Add("Error In Line " + LineCounter.ToString() + " : Value is out of range ( It must be between -32768 and 32767 )");
                        return false;
                    }

                    return true;
                }

                if (data.Substring(0, 5) == "HIGH(" & data[data.Length - 1] == ')')
                {
                    if (data.Length != 11)
                    {
                        Errors.Add("Error In Line " + LineCounter.ToString() + " : Data Length is not correct ( It must consist of 5 decimal number ) ");
                        return false;
                    }

                    if (!Int32.TryParse(data.Substring(5, 5), out parsed_value))
                    {
                        Errors.Add("Error In Line " + LineCounter.ToString() + " : Value between parenthysis is not recognized ");
                        return false;
                    }
                    else if (parsed_value <= -32768 | 32767 <= parsed_value)
                    {
                        Errors.Add("Error In Line " + LineCounter.ToString() + " : Value is out of range ( It must be between -32768 and 32767 )");
                        return false;
                    }

                    return true;
                }
            }

            switch (data[data.Length - 1])
            {
                case 'B'://binary address inserted
                    {
                        if (data.Length != 9)
                        {
                            Errors.Add("Error In Line " + LineCounter.ToString() + " : Data Length is not correct ( It must consist of 8 bits ) ");
                            return false;
                        }

                        foreach (char letter in data.Substring(0, data.Length - 1))
                        {
                            if (letter != '0' & letter != '1')
                            {
                                Errors.Add("Error In Line " + LineCounter.ToString() + " : Data is not in binary format ");
                                return false;
                            }
                        }

                        return true;//Convert.ToInt32(data.Substring(0, data.Length - 1), 2);

                        break;
                    }

                case 'H'://hexadecimal address inserted
                    {
                        if (data.Length != 3)
                        {
                            Errors.Add("Error In Line " + LineCounter.ToString() + " : Data Length is not correct ( It must consist of 2 hexadecimal digits ) ");
                            return false;
                        }
                        else if (!Int32.TryParse(data.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out parsed_value))
                        {
                            Errors.Add("Error In Line " + LineCounter.ToString() + " : Data is not in hexadecimal format ");
                            return false;
                        }

                        return true;//parsed_value;

                        break;
                    }

                default://inserted address is decimal
                    {
                        if (Int32.TryParse(data, out parsed_value))
                        {
                            if (parsed_value > 255 | parsed_value < 0)
                            {
                                Errors.Add("Error In Line " + LineCounter.ToString() + " : Data is out of range ( It must be between 0 and 255 ) ");
                                return false;
                            }

                            return true;//parsed_value;
                        }
                        else
                        {
                            Errors.Add("Error In Line " + LineCounter.ToString() + " : Data is not recognized ");
                            return false;
                        }

                        break;
                    }
            }

            return false;
        }

        public  void Analyze_Text2()
        {
            TextPointer word_start = this.CaretPosition.GetLineStartPosition(0);
            TextPointer LineBegin = this.CaretPosition.GetLineStartPosition(0).GetInsertionPosition(LogicalDirection.Forward);
            TextPointer LineEnd = (this.CaretPosition.GetLineStartPosition(1) != null ? this.CaretPosition.GetLineStartPosition(1) : this.CaretPosition.DocumentEnd).GetInsertionPosition(LogicalDirection.Backward);

            TextRange current_line = new TextRange(LineBegin, LineEnd);
            string[] current_line_words = current_line.Text.Split(trim4, StringSplitOptions.RemoveEmptyEntries);

            Queue words = new Queue();

            //Queue comments = new Queue();
            //bool comment = false;

            string current_word;
            string current_word_o;

            if (current_line_words.Length == 0)
            {
                return;
            }

            for (int i = 0; i < current_line_words.Length; i++)
            {
                if (!current_line_words[i].Contains(";"))
                {
                    words.Enqueue(current_line_words[i]);
                }
                //else if (comment)
                //{
                //    comments.Enqueue(current_line_words[i]);//.Substring(current_line_words[i].IndexOf(";")));
                //}
                else
                {
                    //comment = true;

                    string temp = current_line_words[i].Substring(0, current_line_words[i].IndexOf(";"));

                    if (!String.IsNullOrWhiteSpace(temp))
                    {
                        words.Enqueue(temp);
                    }

                    //comments.Enqueue(current_line_words[i].Substring(current_line_words[i].IndexOf(";")));

                    break;
                }
            }

            if (words.Count == 0)
            {
                goto com;
            }

            current_word_o = (string)words.Dequeue();
            current_word = current_word_o.ToUpperInvariant();

            int index = 0;
            TextRange highLight = null;
            TextRange in_line = null;

            if (current_word[current_word.Length - 1] == ':')
            {
                highLight = new TextRange(word_start , LineEnd);
                index = highLight.Text.IndexOf(current_word_o);
                word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Violet); }else { highLight.ClearAllProperties();}

                if (words.Count != 0)
                {
                    current_word_o = (string)words.Dequeue();
                    current_word = current_word_o.ToUpperInvariant();
                }
                else
                {
                    return;
                }
            }

            if (current_word == ".ORG")
            {
                highLight = new TextRange(word_start, LineEnd);
                index = highLight.Text.IndexOf(current_word_o);
                word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Violet); }else { highLight.ClearAllProperties();}

                if (words.Count != 0)
                {
                    current_word_o = (string)words.Dequeue();

                    highLight = new TextRange(word_start, LineEnd);
                    index = highLight.Text.IndexOf(current_word_o);
                    word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                    if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Violet); }else { highLight.ClearAllProperties();}
                }
                else
                {
                    return;
                }

                if (words.Count != 0)
                {
                    current_word_o = (string)words.Dequeue();
                    current_word = current_word_o.ToUpperInvariant();
                }
                else
                {
                    return;
                }
            }

            if (current_word == ".END")
            {
                word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                highLight = new TextRange(word_start, LineEnd);
                index = highLight.Text.IndexOf(current_word_o);
                word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Violet); } else { highLight.ClearAllProperties();}    
            }
            else if (One_Byte_Instruction.Contains(current_word))
            {
                word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                highLight = new TextRange(word_start, LineEnd);

                index = highLight.Text.IndexOf(current_word_o);
                word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                if(studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, studio_setting.Brush1); } else { highLight.ClearAllProperties();}
                //Refresh_AutoComplete(current_word, -1);
            }
            else if (JUMPs.Contains(current_word) | current_word == "PMOV" | current_word == "STA" | current_word == "LDA")
            {
                word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                highLight = new TextRange(word_start, LineEnd);

                in_line = new TextRange(word_start, LineEnd);
                index = in_line.Text.IndexOf(current_word_o);
                word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, studio_setting.Brush1); } else { highLight.ClearAllProperties();}

                if (words.Count != 0)
                {
                    current_word_o = (string)words.Dequeue();

                    word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                    highLight = new TextRange(word_start, LineEnd);
                    index = highLight.Text.IndexOf(current_word_o);
                    word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                    if(studio_setting.IsSyntaxHighLightingEnabled){highLight.ApplyPropertyValue(TextElement.ForegroundProperty, studio_setting.Brush2);} else { highLight.ClearAllProperties();}

                    if (JUMPs.Contains(current_word))
                    {
                        if (studio_setting.IsAutoCompleteEnabled) { Refresh_AutoComplete(current_word_o, 5); }
                    }
                    else if (current_word == "PMOV")
                    {
                        if (studio_setting.IsAutoCompleteEnabled) { Refresh_AutoComplete(current_word_o.ToUpperInvariant(), 2); }
                    }
                }
            }
            else if (current_word == "PMVI")
            {
                word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                highLight = new TextRange(word_start, LineEnd);
                index = highLight.Text.IndexOf(current_word_o);
                word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, studio_setting.Brush1); } else { highLight.ClearAllProperties();}
                if (studio_setting.IsAutoCompleteEnabled)       { Refresh_AutoComplete(current_word, -1); }

                if (words.Count != 0)
                {
                    current_word_o = (string)words.Dequeue();

                    word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                    highLight = new TextRange(word_start, LineEnd);
                    index = highLight.Text.IndexOf(current_word_o);
                    word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                    if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, studio_setting.Brush2); } else { highLight.ClearAllProperties();}
                    if (studio_setting.IsAutoCompleteEnabled)       { Refresh_AutoComplete(current_word_o.ToUpperInvariant(), 3); }
                }

                if (words.Count != 0)
                {
                    current_word_o = (string)words.Dequeue();

                    word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                    highLight = new TextRange(word_start, LineEnd);
                    index = highLight.Text.IndexOf(current_word_o);
                    word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                    if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, studio_setting.Brush3); } else { highLight.ClearAllProperties();}
                }
            }
            else
            {
                word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                highLight = new TextRange(word_start, LineEnd);
                index = highLight.Text.IndexOf(current_word_o);
                word_start = word_start.GetPositionAtOffset(index + current_word_o.Length);

                if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red); } else { highLight.ClearAllProperties();}
                if (studio_setting.IsAutoCompleteEnabled)       { Refresh_AutoComplete(current_word, -1); }
            }

            foreach (string item in words)
            {
                AutoSense.Visibility = Visibility.Hidden;
                word_start = word_start.GetNextInsertionPosition(LogicalDirection.Forward);
                highLight = new TextRange(word_start, LineEnd);
                index = highLight.Text.IndexOf(item);
                word_start = word_start.GetPositionAtOffset(index + item.Length);

                if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red); } else { highLight.ClearAllProperties();}
            }

        com: highLight = new TextRange(word_start, LineEnd);
            if (studio_setting.IsSyntaxHighLightingEnabled) { highLight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green); } else { highLight.ClearAllProperties(); }
           
            return;
        }

        private void EditNumber(int arg)
        {
            if (arg <= ExNumber)
            {
                return;
            }
            else
            {
                string toAdd = "";

                for (int i = ExNumber + 1; i < arg + 1; i++)
                {
                    toAdd += i.ToString() + "\n";

                }

                Dispatcher.Invoke(new System.Action(() => LineNumber.AppendText(toAdd))); ;
            }

            ExNumber = arg;
            return;
        }

        public  void AddAutoSense(string selection)
        {
            int ileri = 0;
            int geri = 0;
            string backward = this.CaretPosition.GetTextInRun(LogicalDirection.Backward);
            string forward = this.CaretPosition.GetTextInRun(LogicalDirection.Forward);

            if (backward.LastIndexOf(' ') == -1)//there is no space in back 
            {
                if (forward.IndexOf(' ') == -1)
                {
                    geri = -backward.Length;
                    ileri = forward.Length;
                }
                else
                {
                    geri = -backward.Length;
                    ileri = forward.IndexOf(' ');
                }
            }
            else// there is a space in back
            {
                if (forward.IndexOf(' ') == -1)
                {
                    geri = -(backward.Length - backward.LastIndexOf(' ') - 1);
                    ileri = forward.Length;
                }
                else
                {
                    geri = -(backward.Length - backward.LastIndexOf(' ') - 1);
                    ileri = forward.IndexOf(' ');
                }
            }

            this.CaretPosition.DeleteTextInRun(ileri);
            this.CaretPosition.DeleteTextInRun(geri);
            this.CaretPosition.InsertTextInRun(selection + " ");
            TextPointer temp = this.CaretPosition.GetLineStartPosition(1);
            this.CaretPosition = (temp == null) ? this.CaretPosition.DocumentEnd : temp.GetInsertionPosition(LogicalDirection.Backward);

            AutoSense.Visibility = Visibility.Hidden;

            this.Focus();
        }

        private void Refresh_AutoComplete(string CurrentWord, int type)
        {
            AutoSense.Visibility = Visibility.Hidden;
            AutoSense.Items.Clear();

            if (thread_timer.IsAlive)
            {
                thread_timer.Abort();
            }
            
            if (CurrentWord.Length == 0)
            {
                return;
            }

            switch (type)
            {
                case -1://default instruction
                    {
                        if (One_Byte_Instruction.Contains(CurrentWord) | JUMPs.Contains(CurrentWord) | CurrentWord == "PMVI" | CurrentWord == "PMOV")
                        {
                            return;
                        }

                        foreach (string item in One_Byte_Instruction)
                        {
                            if (item.Length < CurrentWord.Length)
                            {
                                continue;
                            }

                            if (item.Substring(0, CurrentWord.Length) == CurrentWord)
                            {
                                AutoSense.Items.Add(item[0] + item.Substring(1).ToLowerInvariant());
                            }
                        }

                        foreach (string item in JUMPs)
                        {
                            if (item.Length < CurrentWord.Length)
                            {
                                continue;
                            }

                            if (item.Substring(0, CurrentWord.Length) == CurrentWord)
                            {
                                AutoSense.Items.Add(item[0] + item.Substring(1).ToLowerInvariant());
                            }
                        }

                        if (CurrentWord.Length <= 4)
                        {
                            if ("PMVI".Substring(0, CurrentWord.Length) == CurrentWord)
                            {
                                AutoSense.Items.Add("Pmvi");
                            }

                            if ("PMOV".Substring(0, CurrentWord.Length) == CurrentWord)
                            {
                                AutoSense.Items.Add("Pmov");
                            }
                        }

                        break;
                    }

                case 2://PMOV
                    {
                        if (PMOVs.Contains(CurrentWord))
                        {
                            return;
                        }

                        foreach (string item in PMOVs)
                        {
                            if (item.Length < CurrentWord.Length)
                            {
                                continue;
                            }
                            if (item.Substring(0, CurrentWord.Length) == CurrentWord)
                            {
                                AutoSense.Items.Add(item[0] + item.Substring(1).ToLowerInvariant());
                            }
                        }

                        break;
                    }

                case 3://PMVI
                    {
                        if (PMVIs.Contains(CurrentWord))
                        {
                            return;
                        }

                        foreach (string item in PMVIs)
                        {
                            if (item.Length < CurrentWord.Length)
                            {
                                continue;
                            }
                            if (item.Substring(0, CurrentWord.Length) == CurrentWord)
                            {
                                AutoSense.Items.Add(item[0] + item.Substring(1).ToLowerInvariant());
                            }
                        }

                        break;
                    }

                case 5://JUMP
                    {
                        if (Labels.Contains(CurrentWord))
                        {
                            return;
                        }

                        foreach (string item in Labels)
                        {
                            //string item2 = item.Split(trim)[0];

                            if (item.Length < CurrentWord.Length)
                            {
                                continue;
                            }
                            if (item.Substring(0, CurrentWord.Length) == CurrentWord)
                            {
                                AutoSense.Items.Add(item[0] + item.Substring(1).ToLowerInvariant());
                            }
                        }

                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            if (AutoSense.Items.Count != 0)
            {
                AutoSense.RenderTransform = new TranslateTransform(this.CaretPosition.GetCharacterRect(LogicalDirection.Backward).X + 5, this.CaretPosition.GetCharacterRect(LogicalDirection.Backward).Y + 10);
                AutoSense.Visibility = Visibility.Visible;
                thread_timer = new Thread(AutoCompleteTimer);
                thread_timer.Start();
            }

            return;
        }

        //--------------------------------------------------------------------->

        private bool   IsThere(string word, int line_n, string[] lines)
        {
            for (int i = 0; i < line_n; i++)
            {
                if (lines[i].Contains(word))
                {
                    if (lines[i].IndexOf(word) == 0)
                    {
                        return false;
                    }
                    else if (lines[i][lines[i].IndexOf(word) - 1] == '\r' | lines[i][lines[i].IndexOf(word) - 1] == '\n')
                    {
                        return false;
                    }
                }

                continue;
            }

            return true;
        }

        private int    search(int j)
        {
            if (j == 15)
            {
            }

            for (int i = 0; i < 4096; i++)
            {
                if (UsedAddress[i] == j)
                {
                    return i;
                }
            }

            return -1;
        }

        private int    check(string word)
        {
            word = word.ToUpperInvariant();

            if (One_Byte_Instruction.Contains(word))
            {
                return 1;
            }
            else if (JUMPs.Contains(word))
            {
                return 2;
            }
            else if (word == "PMVI")
            {
                return 3;
            }
            else if (word == "PMOV")
            {
                return 4;
            }
            else if (word == "LDA" | word == "STA")
            {
                return 5;
            }

            return 0;
        }

        private int    opcode(string word)
        {
            word = word.ToUpperInvariant();

            if (word == "ADD")
            {
                return 16;
            }
            else if (word == "SUB")
            {
                return 17;
            }
            else if (word == "CMP")
            {
                return 18;
            }
            else if (word == "DEC")
            {
                return 19;
            }
            else if (word == "AND")
            {
                return 20;
            }
            else if (word == "OR")
            {
                return 21;
            }
            else if (word == "XOR")
            {
                return 22;
            }
            else if (word == "CPTA")
            {
                return 23;
            }
            else if (word == "RORA")
            {
                return 24;
            }
            else if (word == "ROLA")
            {
                return 25;
            }
            else if (word == "RORC")
            {
                return 26;
            }
            else if (word == "ROLC")
            {
                return 27;
            }
            else if (word == "CLC")
            {
                return 28;
            }
            else if (word == "CPTC")
            {
                return 29;
            }
            else if (word == "INC")
            {
                return 30;
            }
            else if (word == "HLT")
            {
                return 31;
            }
            else if (word == "NOOP")
            {
                return 36;
            }
            else if (word == "SWP")
            {
                return 37;
            }
            else if (word == "INA")
            {
                return 38;
            }
            else if (word == "OUTA")
            {
                return 39;
            }
            else if (word == "LDA")
            {
                return 64;
            }
            else if (word == "STA")
            {
                return 80;
            }
            else if (word == "JMP")
            {
                return 128;
            }
            else if (word == "JZR")
            {
                return 144;
            }
            else if (word == "JNZ")
            {
                return 160;
            }
            else if (word == "JCY")
            {
                return 176;
            }
            else if (word == "JNC")
            {
                return 192;
            }
            else if (word == "JNG")
            {
                return 208;
            }
            else if (word == "JPS")
            {
                return 224;
            }
            else if (word == "JOV")
            {
                return 240;
            }
            else
            {
                return 299;
            }
        }
        
        private int    chksum(int in_1, int in_2)
        {
            int output;
            output = 255 - in_1 - (in_2 / 256) - (in_2 % 256);

            while (output < 0)
            {
                output = output + 256;
            }

            return output;
        }

        private int    search(string word, string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(word + ":"))
                {
                    return UsedAddress[i];
                }

            }

            return 5000;

        }//return label address

        private void   Add(int linenumber , int address , string opcode , string o_line)//8 9 8
        {
            TextRange append = null;
            Dispatcher.Invoke(new System.Action(() => append = new TextRange(Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward), Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward))));
            Dispatcher.Invoke(new System.Action(() => append.Text = Format("", 3)));
            //*************************
            Dispatcher.Invoke(new System.Action(() => append = new TextRange(Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward), Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward))));
            Dispatcher.Invoke(new System.Action(() => append.Text = Format(linenumber.ToString("D4"),13)));
            Dispatcher.Invoke(new System.Action(() => append.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.YellowGreen )));
            //*************************
            Dispatcher.Invoke(new System.Action(() => append = new TextRange(Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward), Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward))));
            Dispatcher.Invoke(new System.Action(() => append.Text = Format(address.ToString("X3"),11)));
            Dispatcher.Invoke(new System.Action(() => append.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.HotPink )));
            //*************************
            Dispatcher.Invoke(new System.Action(() => append = new TextRange(Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward), Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward))));
            Dispatcher.Invoke(new System.Action(() => append.Text = Format(opcode,8)));
            Dispatcher.Invoke(new System.Action(() => append.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue )));
            //*************************
            Dispatcher.Invoke(new System.Action(() => append = new TextRange(Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward), Result.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward))));
            Dispatcher.Invoke(new System.Action(() => append.Text = o_line+"\n"));
            Dispatcher.Invoke(new System.Action(() => append.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Violet)));
            //*************************
        }

        private void   Add(int address , string data , string checksum)
        {
            HexFileContent += ":010" + address.ToString("X3") + "00" + data + checksum + "\n";
            ParkamHexFileContent += Format("", 10) + Format(address.ToString("X3"), 10) + Format("", 10) + Format(data, 10) + "\n";
        }

        private void   End()
        {
            HexFileContent += ":00000001FF";
            Dispatcher.Invoke(new System.Action(() => Memory.Text = ParkamHexFileContent));
        }

        private void   Clear()
        {
            HexFileContent       = "";
            ParkamHexFileContent = "";

            Dispatcher.Invoke(new System.Action(() => Result.Document.Blocks.Clear()));
        }

        private void   Append_C(string text, Brush color)
        {
            TextRange append = null;
            Dispatcher.Invoke(new System.Action(() => append = new TextRange(this.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward), this.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Forward))));
            Dispatcher.Invoke(new System.Action(() => append.Text = text));
            Dispatcher.Invoke(new System.Action(() => append.ApplyPropertyValue(TextElement.ForegroundProperty, color)));
        }

        private string Format(string text, int num)
        {
            int k = text.Length;

            for (int j = k; j < num; j++)
            {
                text += " ";
            }

            return text;
        }

        private Queue  Format(string[] code_Lines)
        {
            int max = 0;//maximum label length
            Queue return_parameter = new Queue();

            foreach (string item in Labels)
            {
                if (item.Length > max)
                {
                    max = item.Length;
                }
            }

            max += 2;

            for (int j = 0; j < code_Lines.Length; j++)
            {
                string temp = "";
                string comment = "";

                if (code_Lines[j].IndexOf(";") != -1)
                {
                    comment = code_Lines[j].Substring(code_Lines[j].IndexOf(";"));

                    code_Lines[j] = code_Lines[j].Substring(0, code_Lines[j].IndexOf(";"));
                }

                if (String.IsNullOrWhiteSpace(code_Lines[j]) | code_Lines[j].ToUpperInvariant().Contains(".ORG"))
                {
                    continue;
                }

                string[] text_words = code_Lines[j].Split(trim, StringSplitOptions.RemoveEmptyEntries);

                if (text_words[0][text_words[0].Length - 1] == ':')
                {
                    temp += Format(text_words[0], max);

                    temp += Format(text_words[1], 6);

                    if (text_words.Length > 2)
                    {
                        for (int i = 2; i < text_words.Length; i++)
                        {
                            temp += Format(text_words[i], 6) ;
                        }
                    }
                }
                else
                {
                    temp += Format("", max) + Format(text_words[0], 6) ;

                    if (text_words.Length > 1)
                    {
                        for (int i = 1; i < text_words.Length; i++)
                        {
                            temp += Format(text_words[i], 6);
                        }
                    }
                }

                //temp += comment;

                return_parameter.Enqueue(temp);
            }

            return return_parameter;
        }

        //------------------------------------------------------------------>Methods executed by other threads

        private void Load             ()
        {
            thread_check.Suspend();
            Dispatcher.Invoke(new System.Action(() => AutoSense.Visibility = Visibility.Hidden));

            string extension = System.IO.Path.GetExtension(OpenFile.FileName).ToLowerInvariant();

            switch (extension)
            {
                case ".txt":
                case ".asm":
                case ".pkm":
                    {
                        StreamReader load = new StreamReader(OpenFile.OpenFile());
                        string whole_text = load.ReadToEnd();
                        string[] whole_text_line = whole_text.Split(trim3, StringSplitOptions.RemoveEmptyEntries);

                        Dispatcher.Invoke(new System.Action(() => this.Document.Blocks.Clear()));
                        Dispatcher.Invoke(new System.Action(() => StatusBar.Maximum = whole_text_line.Length));

                        for (int i = 0; i < whole_text_line.Length; i++)
                        {
                            string comment = "";
                            Queue words = new Queue();

                            if (whole_text_line[i].IndexOf(";") != -1)
                            {
                                comment = whole_text_line[i].Substring(whole_text_line[i].IndexOf(";"));
                                whole_text_line[i] = whole_text_line[i].Substring(0, whole_text_line[i].IndexOf(";"));
                            }
                            else
                            {
                                comment = "";
                            }

                            string[] item_words = whole_text_line[i].Split(trim, StringSplitOptions.RemoveEmptyEntries);

                            if (item_words.Length == 0)
                            {
                                continue;
                            }

                            foreach (string item2 in item_words)
                            {
                                words.Enqueue(item2);
                            }

                            string current_word_temp = (string)words.Dequeue();
                            string current_word = current_word_temp.ToUpperInvariant();

                            //check whether this line is labeled or not
                            if (current_word.IndexOf(':') == ((current_word.Length == 0) ? -2 : current_word.Length - 1))
                            {
                                Append_C(current_word_temp, Brushes.Violet);
                                Append_C(" ", Brushes.Black);

                                if (words.Count != 0)
                                {
                                    current_word_temp = (string)words.Dequeue();
                                    current_word = current_word_temp.ToUpperInvariant();
                                }
                                else
                                {
                                    goto end_line;
                                }
                            }

                            if (One_Byte_Instruction.Contains(current_word))
                            {
                                Append_C(current_word_temp, studio_setting.Brush1);
                                Append_C(" ", Brushes.Black);
                            }
                            else if (JUMPs.Contains(current_word) | current_word == "PMOV")
                            {
                                Append_C(current_word_temp, studio_setting.Brush1);
                                Append_C(" ", Brushes.Black);

                                if (words.Count != 0)
                                {
                                    Append_C((string)words.Dequeue(), studio_setting.Brush2);
                                    Append_C(" ", Brushes.Black);
                                }
                                else
                                {
                                    goto end_line;
                                }
                            }
                            else if (current_word == "PMVI")
                            {
                                Append_C(current_word_temp, studio_setting.Brush1);
                                Append_C(" ", Brushes.Black);

                                if (words.Count != 0)
                                {
                                    Append_C((string)words.Dequeue(), studio_setting.Brush2);
                                    Append_C(" ", Brushes.Black);
                                }
                                else
                                {
                                    goto end_line;
                                }

                                if (words.Count != 0)
                                {
                                    Append_C((string)words.Dequeue(), studio_setting.Brush3);
                                    Append_C(" ", Brushes.Black);
                                }
                                else
                                {
                                    goto end_line;
                                }
                            }
                            else if (current_word == "STA" | current_word == "LDA")
                            {
                                Append_C(current_word_temp, studio_setting.Brush1);
                                Append_C(" ", Brushes.Black);

                                if (words.Count != 0)
                                {
                                    Append_C((string)words.Dequeue(), studio_setting.Brush2);
                                    Append_C(" ", Brushes.Black);
                                }
                                else
                                {
                                    goto end_line;
                                }
                            }
                            else if (current_word == ".ORG")
                            {
                                Append_C(current_word_temp, Brushes.Violet);
                                Append_C(" ", Brushes.Black);

                                if (words.Count != 0)
                                {
                                    Append_C((string)words.Dequeue(), Brushes.Violet);
                                    Append_C(" ", Brushes.Black);
                                }
                                else
                                {
                                    goto end_line;
                                }
                            }
                            else if (current_word == ".END")
                            {
                                Append_C(current_word_temp, Brushes.Violet);
                                Append_C(" ", Brushes.Black);
                            }
                            else
                            {
                                Append_C(current_word_temp, Brushes.Black);
                                Append_C(" ", Brushes.Black);
                            }

                            while (words.Count != 0)
                            {
                                Append_C((string)words.Dequeue(), Brushes.Black);
                                Append_C(" ", Brushes.Black);
                            }

                            Append_C("    " + comment, Brushes.Green);
                        end_line: Append_C("\n", Brushes.Black);

                            Dispatcher.Invoke(new System.Action(() => StatusBar.Value++));
                            Dispatcher.Invoke(new System.Action(() => Status.Text = "Loaded " + i.ToString() + "." + "line"));
                        }

                        load.Close();

                        break;
                    }

                case ".pkmlst":
                    {
                        TextRange Content = new TextRange(Result.Document.ContentStart, Result.Document.ContentEnd);
                        Dispatcher.Invoke(new System.Action(() => Content.Load(OpenFile.OpenFile(), DataFormats.Rtf.ToString())));

                        break;
                    }


                case ".pkmhex":
                    {
                        StreamReader Content = new StreamReader(OpenFile.FileName);
                        Dispatcher.Invoke(new System.Action(() => Memory.Text = Content.ReadToEnd()));
                        Content.Close();

                        break;
                    }

                case ".res":
                    {
                        StreamReader Content = new StreamReader(OpenFile.FileName);
                        Dispatcher.Invoke(new System.Action(() => AnalyzerResult.Text = Content.ReadToEnd()));
                        Content.Close();

                        break;
                    }

                default:
                    {
                        StreamReader Content = new StreamReader(OpenFile.FileName);
                        Dispatcher.Invoke(new System.Action(() => Other.Text = Content.ReadToEnd()));
                        Content.Close();

                        break;
                    }

            }


            Dispatcher.Invoke(new System.Action(() => StatusBar.Value = 0));
            thread_check.Resume();
        }

        private void Check            ()
        {
            bool SearchForDotOrg;
            bool SearchForDotEnd;

            string text = "";
            string[] text_lines;
            ArrayList toDelete = new ArrayList();

            while (true)
            {
                SearchForDotOrg = true;
                SearchForDotEnd = true;

                Errors.Clear();
                Warning.Clear();

                text = "";

                try
                {
                    Dispatcher.Invoke(new System.Action(() => text = new TextRange(this.CaretPosition.DocumentStart.GetInsertionPosition(LogicalDirection.Forward), this.CaretPosition.DocumentEnd.GetInsertionPosition(LogicalDirection.Backward)).Text));
                }
                catch
                {
                    continue;
                }

                text_lines = text.Split(trim3, StringSplitOptions.RemoveEmptyEntries);

                foreach (string item in Labels)
                {
                    string temp2 = item.Split(trim)[0] + ":";

                    if (!text.Contains(temp2))
                    {
                        toDelete.Add(item);
                    }
                }

                foreach (string item in toDelete)
                {
                    Labels.Remove(item);
                }

                toDelete.Clear();

                //------------------------------------------------------------------>

                int temp = 0;
                int Address = 0;
                UsedAddress[0] = 0;

                int i;

                for (i = 0; i < text_lines.Length; i++)
                {
                    int indice = text_lines[i].IndexOf(";");

                    if (indice != -1)
                    {
                        text_lines[i] = text_lines[i].Substring(0, indice);
                    }

                    if (String.IsNullOrWhiteSpace(text_lines[i]))
                    {
                        int temp3 = UsedAddress[i];
                        UsedAddress[i] = -2;
                        Address = temp3;
                        UsedAddress[i + 1] = Address;
                        continue;
                    }
                    else
                    {
                        if (SearchForDotOrg)
                        {
                            if (!text_lines[i].ToUpperInvariant().Contains(".ORG"))
                            {
                                Warning.Add("Warning in Line " + (i + 1).ToString() + " : .ORG pseudo instruction not found , assembling will start from 000H");
                            }

                            SearchForDotOrg = false;
                        }

                        if (SearchForDotEnd)
                        {
                            if (text_lines[i].ToUpperInvariant().Contains(".END"))
                            {
                                SearchForDotEnd = false;

                                if (i != text_lines.Length - 1)
                                {
                                    Warning.Add("Warning in Line " + (i + 1).ToString() + " : Instructions after .END pseudo instruction won't be assembled");
                                }
                            }
                        }
                    }

                    temp = Analyze_Line2(text_lines[i], text, i + 1, text_lines);

                    if (temp == -1)
                    {
                        Address = -1;
                        UsedAddress[i + 1] = -1;
                    }
                    else if (temp == -3)
                    {
                        UsedAddress[i] = -3;

                        if (i != text_lines.Length - 1)
                        {
                            Warning.Add("Instructions after .end pseudo instruction won't be assembled");
                        }

                        break;
                    }
                    else if (temp < 3)
                    {
                        if (Address != -1)
                        {
                            for (int j = 0; j < i; j++)
                            {
                                if (UsedAddress[j] == Address)
                                {
                                    Warning.Add("Warning in Line " + (i + 1).ToString() + " : This memory address is used before ( First one will be assembled )");
                                }
                            }

                            Address += temp;
                            UsedAddress[i + 1] = Address;
                        }
                        else
                        {
                            Address = -1;
                            UsedAddress[i + 1] = -1;
                        }
                    }
                    else
                    {
                        UsedAddress[i] = -2;
                        Address = temp - 2000;
                        UsedAddress[i + 1] = Address;
                    }
                }

                if (SearchForDotEnd)
                {
                    Errors.Add("Error in Line " + text_lines.Length.ToString() + " : .END pseudo instruction not found");
                }

                ListError.Update(Errors);
                ListWarning.Update(Warning);
                ListMessage.Update(Errors, Warning);
                //----------------------------------------------------------------------->

                LineHelper.Refresh2(UsedAddress, i + 1);
                EditNumber(text_lines.Length);
                Thread.Sleep(500);
            }
        }

        private void Analyzer         ()
        {
            Queue InputValues = new Queue();
            TimeAnalyzer temp = new TimeAnalyzer();
            AnalyzeResult res = temp.Analyze(InputValues , new Info(hexa));

            if (res.NumberOfINS == -1)
            {
                MessageBox.Show("Analyzer discovered infinte loop , unable to calculate execution time ");
                return;
            }

            if (res.NumberOfINA != 0)
            {
                string result = Microsoft.VisualBasic.Interaction.InputBox("Analyzer discovered " + res.NumberOfINA.ToString() + " INA instructions , to run analyzer with specific input values enter them below , click on Cancel to run analyzer with default 0 value", "Input", "Default data", -1, -1);

                if (!String.IsNullOrWhiteSpace(result))
                {
                    string[] input_values = result.Split(trim4, StringSplitOptions.RemoveEmptyEntries);
                    InputValues.Clear();
                    byte counter3 = 0;


                    for (int i = 0; i < input_values.Length; i++)
                    {
                        try
                        {
                            if (input_values[i].Contains("HIGH(") & input_values[i][input_values[i].Length - 1] == ')')   //HIGH()
                            {
                                input_values[i] = input_values[i].Substring(5, result.Length - 6);
                                int counter4 = Convert.ToInt32(input_values[i], 10);

                                if (counter4 < 0)
                                {
                                    counter4 += 65536;
                                }

                                counter4 /= 256;
                                counter3 = (byte)counter4;
                            }
                            else if (input_values[i].Contains("LOW(") & input_values[i][input_values[i].Length - 1] == ')') //LOW()
                            {
                                input_values[i] = input_values[i].Substring(4, input_values[i].Length - 5);
                                int counter4 = Convert.ToInt32(input_values[i], 10);

                                if (counter4 < 0)
                                {
                                    counter4 += 65536;
                                }

                                counter4 %= 256;
                                counter3 = (byte)counter4;
                            }
                            else if (input_values[i][input_values[i].Length - 1] == 'B')//binary
                            {
                                input_values[i] = input_values[i].Substring(0, input_values[i].Length - 1);
                                counter3 = Convert.ToByte(input_values[i], 2);
                            }
                            else if (input_values[i][input_values[i].Length - 1] == 'H')//hexadecimal
                            {
                                input_values[i] = input_values[i].Substring(0, input_values[i].Length - 1);
                                counter3 = Convert.ToByte(input_values[i], 16);
                            }
                            else//decimal
                            {
                                int counter4 = Convert.ToByte(input_values[i], 10);

                                if (counter4 < 0)
                                {
                                    counter4 += 256;
                                }

                                counter3 = (byte)counter4;

                            }//decimal ends

                            InputValues.Enqueue(counter3);

                        }
                        catch
                        {
                            InputValues.Enqueue(0);
                        }
                    }

                    temp = new TimeAnalyzer();
                    res = temp.Analyze(InputValues,new Info(hexa));
                }
            }

            string text = "";

            text += "    Result  of Analyze :     " + "\n\n";
            text += "      Assembly Code toke     " + res.NumberOfCycles.ToString("D6")        + " Tcycles to execute " + "\n";
            text += "      Assembly Code executed " + res.NumberOfINS.ToString("D6")           + " instructions in " + ((double)(res.NumberOfCycles / 27)).ToString("F6") + " us" + "\n\n\n";

            text += "    Status  of Flags     by end of execution : "   + "\n\n";
            text += "                 Zero  : "     + res.Memory.Z_Flag                        + "      Negative : " + res.Memory.N_Flag + "\n";
            text += "                Carry  : "     + res.Memory.C_Flag                        + "      Overflow : " + res.Memory.O_Flag + "\n\n\n";
            

            text += "   Content  of Registers by end of execution : "   + "\n\n";
            text += "                    A  :  "    + res.Memory.A_Content.ToString("X2")      +  "               B : " + res.Memory.B_Content.ToString("X2")    + "\n";
            text += "                    X  :  "    + res.Memory.X_Content.ToString("X2")      +  "          Output : " + res.Memory.Out_Content.ToString("X2")  + "\n";
            text += "            High Temp  :   "   + res.Memory.HTemp_Content.ToString("X1")  +  "        Low Temp : " + res.Memory.LTemp_Content.ToString("X2")+ "\n";
            text += "      Program Counter  : "     + res.Memory.PC_Content.ToString("X3")     + "\n\n\n";


            text += "   Content  of Memory    by end of execution : "   + "\n\n";
            
            for (int i = 0; i < 4096; i++)
            {
            text += "             M [ " + i.ToString("X3") + " ] : " + res.Memory.RAM_Content[i].ToString("X2") + "        M [ " + (++i).ToString("X3") + " ] : " + res.Memory.RAM_Content[i].ToString("X2") + "\n"; 
            }

            Dispatcher.Invoke(new System.Action(() => AnalyzerResult.Text = text));
        }

        private void Assembler()
        {
            int word1;
            int word3;
            int word4;
            int counter3 = 0;

            string word2;
            string namein;
            string Rcheck;
            string Ropcode;
            string temporary;

            Clear();
            Queue WORD = new Queue();

            TextRange wholetext = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);

            string assembletext = wholetext.Text.ToUpperInvariant();

            int start_index = assembletext.IndexOf(".END");
            assembletext = wholetext.Text.Substring(0, start_index);

            string[] codelines = assembletext.Split(trim3, StringSplitOptions.RemoveEmptyEntries);
            string[] codelines_o = assembletext.Split(trim3, StringSplitOptions.RemoveEmptyEntries);

            Dispatcher.Invoke(new System.Action(() => Status.Text = "Parsing Code Lines"));
            Queue O_Lines = Format(codelines);

            Dispatcher.Invoke(new System.Action(() => StatusBar.Maximum = 4096 + codelines.Length));

            for (int i = 0; i < codelines.Length; i++)
            {
                Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                if (codelines[i].IndexOf(";") != -1)
                {
                    codelines[i] = codelines[i].Substring(0, codelines[i].IndexOf(";"));
                }

                if (codelines[i].IndexOf(":") != -1)
                {
                    codelines[i] = codelines[i].Substring(codelines[i].IndexOf(":") + 1);
                }
            }

            Dispatcher.Invoke(new System.Action(() => Status.Text = "Running Assembler"));
            MemoryContent.Clear();

            for (int j = 0; j < 4096; j++)
            {
                int i = search(j);//search for address j --> address j is in line i

                if (i == -1 | i >= codelines.Length)
                {
                    Add(j, "00", chksum(0, j).ToString("X2"));
                    MemoryContent.Add(j.ToString("X3") + " : " + "00");
                    hexa[j] = 0;

                    Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));
                    continue;
                }

                string[] temp = codelines[i].Split(trim, StringSplitOptions.RemoveEmptyEntries);

                foreach (string item in temp)
                {
                    WORD.Enqueue(item);
                }

                namein = (string)WORD.Dequeue();//instruction

                if (check(namein) == 1)      //one byte instructions
                {
                    Ropcode = opcode(namein).ToString("X2");
                    Rcheck = chksum(opcode(namein), j).ToString("X2");

                    Add(j, Ropcode, Rcheck);
                    MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + namein);
                    hexa[j] = Convert.ToByte(Ropcode, 16);

                    Add(i, j, Ropcode, (string)O_Lines.Dequeue());

                    Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                }//case 1 ends

                else if (check(namein) == 2)  //Jump instructions
                {

                    word1 = opcode(namein);
                    word2 = (string)WORD.Dequeue();

                    if (word2 == "$")
                    {
                        word4 = word1 + (j / 256);
                        Ropcode = word4.ToString("X2");
                        Rcheck = chksum(word4, j).ToString("X2");
                        temporary = Ropcode;

                        Add(j, Ropcode, Rcheck);
                        MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + namein);
                        hexa[j] = Convert.ToByte(Ropcode, 16);

                        Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                        j++;
                        //second byte of instruction
                        word4 = ((j - 1) % 256);
                        Ropcode = word4.ToString("X2");
                        Rcheck = chksum(word4, j).ToString("X2");

                        Add(j, Ropcode, Rcheck);
                        MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + "2nd Byte");
                        hexa[j] = Convert.ToByte(Ropcode, 16);

                        Add(i, j - 1, temporary + Ropcode, (string)O_Lines.Dequeue());

                        Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                        continue;   //go to next line
                    }

                    word3 = search(word2, codelines_o);

                    //first byte of instruction
                    word4 = word1 + (word3 / 256);
                    Ropcode = word4.ToString("X2");
                    Rcheck = chksum(word4, j).ToString("X2");
                    temporary = Ropcode;

                    Add(j, Ropcode, Rcheck);
                    MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + namein);
                    hexa[j] = Convert.ToByte(Ropcode, 16);

                    Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                    j++;

                    //second byte of instruction
                    word4 = word3 % 256;
                    Ropcode = word4.ToString("X2");
                    Rcheck = chksum(word4, j).ToString("X2");

                    Add(j, Ropcode, Rcheck);
                    MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + "2nd Byte");
                    hexa[j] = Convert.ToByte(Ropcode, 16);

                    Add(i, j - 1, temporary + Ropcode, (string)O_Lines.Dequeue());

                    Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                }//case 2 ends

                else if (check(namein) == 3)  //PMVI instruction
                {
                    word2 = (string)WORD.Dequeue();   //'A,' or 'X,'
                    word2 = word2.ToUpperInvariant();

                    if (word2 == "A,")
                    {
                        Add(j, "60", chksum(96, j).ToString("X2"));
                        MemoryContent.Add(j.ToString("X3") + " : " + "60" + " : " + "PMVI A");
                        hexa[j] = 96;

                        Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                        j++;

                        namein = (string)WORD.Dequeue();  //Immediate data
                        namein = namein.ToUpperInvariant();

                        if (namein.Contains("HIGH(") & namein[namein.Length - 1] == ')')   //HIGH()
                        {
                            namein = namein.Substring(5, namein.Length - 6);
                            counter3 = Convert.ToInt32(namein, 10);

                            if (counter3 < 0)
                            {
                                counter3 += 65536;
                            }

                            counter3 /= 256;

                        }

                        else if (namein.Contains("LOW(") & namein[namein.Length - 1] == ')') //LOW()
                        {
                            namein = namein.Substring(4, namein.Length - 5);
                            counter3 = Convert.ToInt32(namein, 10);

                            if (counter3 < 0)
                            {
                                counter3 += 65536;
                            }

                            counter3 %= 256;

                        }

                        else if (namein[namein.Length - 1] == 'B')//binary
                        {
                            namein = namein.Substring(0, namein.Length - 1);
                            counter3 = Convert.ToInt32(namein, 2);
                        }

                        else if (namein[namein.Length - 1] == 'H')//hexadecimal
                        {
                            namein = namein.Substring(0, namein.Length - 1);
                            counter3 = Convert.ToInt32(namein, 16);
                        }

                        else//decimal
                        {
                            counter3 = Convert.ToInt32(namein, 10);

                            if (counter3 < 0)
                            {
                                counter3 += 256;
                            }

                        }//decimal ends

                        Ropcode = counter3.ToString("X2");
                        Rcheck = chksum(counter3, j).ToString("X2");

                        Add(j, Ropcode, Rcheck);
                        MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + "2nd Byte");
                        hexa[j] = Convert.ToByte(Ropcode, 16);

                        Add(i, j - 1, "60" + Ropcode, (string)O_Lines.Dequeue());

                        Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                    }//if A ends

                    else if (word2 == "X,")
                    {
                        Add(j, "70", chksum(112, j).ToString("X2"));
                        MemoryContent.Add(j.ToString("X3") + " : " + "70" + " : " + "PMVI X");
                        hexa[j] = 112;

                        Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                        j++;

                        namein = (string)WORD.Dequeue();  //Immediate data

                        if (namein.Contains("HIGH(") & namein[namein.Length - 1] == ')')   //HIGH()
                        {
                            namein = namein.Substring(5, namein.Length - 6);
                            counter3 = Convert.ToInt32(namein, 10);

                            if (counter3 < 0)
                            {
                                counter3 += 65536;
                            }

                            counter3 /= 256;

                        }

                        else if (namein.Contains("LOW(") & namein[namein.Length - 1] == ')') //LOW()
                        {
                            namein = namein.Substring(4, namein.Length - 5);
                            counter3 = Convert.ToInt32(namein, 10);

                            if (counter3 < 0)
                            {
                                counter3 += 65536;
                            }

                            counter3 %= 256;

                        }

                        else if (namein[namein.Length - 1] == 'B')//binary
                        {
                            namein = namein.Substring(0, namein.Length - 1);
                            counter3 = Convert.ToInt32(namein, 2);
                        }

                        else if (namein[namein.Length - 1] == 'H')//hexadecimal
                        {
                            namein = namein.Substring(0, namein.Length - 1);
                            counter3 = Convert.ToInt32(namein, 16);
                        }

                        else//decimal
                        {
                            counter3 = Convert.ToInt32(namein, 10);

                            if (counter3 < 0)
                            {
                                counter3 += 256;
                            }

                        }//decimal ends

                        Ropcode = counter3.ToString("X2");
                        Rcheck = chksum(counter3, j).ToString("X2");

                        Add(j, Ropcode, Rcheck);
                        MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + "2nd Byte");
                        hexa[j] = Convert.ToByte(Ropcode, 16);

                        Add(i, j - 1, "70" + Ropcode, (string)O_Lines.Dequeue());

                        Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                    }//if X ends

                }//case 3 ends

                else if (check(namein) == 4)  //PMOV instruction
                {
                    word2 = (string)WORD.Dequeue();
                    word2 = word2.ToUpperInvariant();

                    if (word2 == "A,B")       //PMOV A,B
                    {
                        counter3 = 32;
                        hexa[j] = 32;

                        Add(i, j, "20", (string)O_Lines.Dequeue());
                        MemoryContent.Add(j.ToString("X3") + " : " + "20" + " : " + "PMOV A,B");
                    }

                    else if (word2 == "B,A")  //PMOV B,A
                    {
                        counter3 = 33;
                        hexa[j] = 33;

                        Add(i, j, "21", (string)O_Lines.Dequeue());
                        MemoryContent.Add(j.ToString("X3") + " : " + "21" + " : " + "PMOV B,A");
                    }

                    else if (word2 == "A,X")  //PMOV A,X
                    {
                        counter3 = 34;
                        hexa[j] = 34;

                        Add(i, j, "22", (string)O_Lines.Dequeue());
                        MemoryContent.Add(j.ToString("X3") + " : " + "22" + " : " + "PMOV A,X");
                    }
                    else if (word2 == "X,A")  //PMOV X,A
                    {
                        counter3 = 35;
                        hexa[j] = 35;

                        Add(i, j, "23", (string)O_Lines.Dequeue());
                        MemoryContent.Add(j.ToString("X3") + " : " + "23" + " : " + "PMOV X,A");
                    }

                    Add(j, counter3.ToString("X2"), chksum(counter3, j).ToString("X2"));

                    Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                }// case 4 ends

                else if (check(namein) == 5)      //Load - Store instructions 
                {
                    word2 = (string)WORD.Dequeue();

                    if (word2[word2.Length - 1] == 'B')
                    {
                        word1 = Convert.ToInt32(word2.Substring(0, word2.Length - 1), 2);
                    }
                    else if (word2[word2.Length - 1] == 'H')
                    {
                        word1 = Convert.ToInt32(word2.Substring(0, word2.Length - 1), 16);
                    }
                    else
                    {
                        word1 = Convert.ToInt32(word2, 10);
                    }

                    word3 = opcode(namein) + (word1 / 256);
                    Ropcode = word3.ToString("X2");
                    temporary = Ropcode;
                    Rcheck = chksum(word3, j).ToString("X2");

                    Add(j, Ropcode, Rcheck);
                    MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + namein);
                    hexa[j] = Convert.ToByte(Ropcode, 16);

                    Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                    j++;

                    word3 = word1 % 256;
                    Ropcode = word3.ToString("X2");
                    Rcheck = chksum(word3, j).ToString("X2");

                    Add(j, Ropcode, Rcheck);
                    MemoryContent.Add(j.ToString("X3") + " : " + Ropcode + " : " + "2nd Byte");
                    hexa[j] = Convert.ToByte(Ropcode, 16);

                    Add(i, j - 1, temporary + Ropcode, (string)O_Lines.Dequeue());

                    Dispatcher.Invoke(new System.Action(() => StatusBar.Value += 1));

                }//case 5 ends

            }//foreach ends

            End();

            MessageBox.Show("Assembling completed");

            Dispatcher.Invoke(new System.Action(() => StatusBar.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Status.Text = "Assembling finished"));

            return;
        }

        private void AutoCompleteTimer()
        {
            Thread.Sleep(studio_setting.AutoCompleteDisplay);
            Dispatcher.Invoke(new System.Action(() => AutoSense.Visibility = Visibility.Hidden));
        }

        //------------------------------------------------------------------>Public Methods

        public void Save    ()
        {
            TextRange Content = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);
            FileStream save = null;

            try
            {
                save = new FileStream(Directory, FileMode.Create);
            }
            catch (IOException)
            {
                return;
            }

            string extension = System.IO.Path.GetExtension(Directory);

            if (extension.ToLowerInvariant() == ".txt" | extension.ToLowerInvariant() == ".pkm" | extension.ToLowerInvariant() == ".asm")
            {
                Content.Save(save, DataFormats.Text.ToString());
            }
            else if (extension.ToLowerInvariant() == ".rtf")
            {
                Content.Save(save, DataFormats.Rtf.ToString());
            }
            else
            {
                MessageBox.Show("Error: Selected File Type is not supported");
            }

            save.Close();
        }

        public void Print   ()
        {
            PrintFile.PageRangeSelection = PageRangeSelection.AllPages;
            PrintFile.UserPageRangeEnabled = true;

            // Display the dialog. This returns true if the user presses the Print button.
            Nullable<Boolean> print = PrintFile.ShowDialog();
            if (print == true)
            {
                PrintFile.PrintVisual(this as Visual, "printing as visual");
                PrintFile.PrintDocument((((IDocumentPaginatorSource)this.Document).DocumentPaginator), "printing as paginator");
                //PrintFile.PrintDocument(fixedDocSeq.DocumentPaginator, "Test print job");
            }
            else
            {
                MessageBox.Show("Error occured during printing");
            }
        }

        public void Closing ()
        {
            thread_check.Abort();

            if (thread_timer != null)
            {
                thread_timer.Abort();
            }

            if (thread_load != null)
            {
                thread_load.Abort();
            }

            if (thread_check != null)
            {
                thread_check.Abort();
            }
        }

        public void Analyze ()
        {
            thread_analyze = new Thread(Analyzer);
            thread_analyze.Start();
        }

        public bool Assemble()
        {
            if (IsCreated)
            {
                if (ListError.Items.Count == 0)
                {
                    if (!IsSaved)
                    {
                        Save();
                        IsSaved = true;
                    }

                    thread_assemble = new Thread(Assembler);
                    thread_assemble.Start();
                    return true;
                }
                else
                {
                    if (ListError.Items.Count == 1)
                    {
                        MessageBox.Show("There is a error in Code , assembler can't be started");
                    }
                    else
                    {
                        MessageBox.Show("There are errors in Code , assembler can't be started");
                    }

                    return false;
                }
            }
            else
            {
                MessageBox.Show("There is no text file to be assembled");
                return false;
            }

        }

        public bool SaveAs(int i)
        {
            switch (i)
            {
                case 0:
                    {
                        SaveFile.Filter = "Parkam Files (*.pkm)|*.pkm|Assembly Files (*.asm)|*.asm|Text Files (*.txt)|*.txt";
                        break;
                    }

                case 1:
                    {
                        SaveFile.Filter = "Parkam Lst Files (*.pkmlst)|*.pkmlst|Lst Files (*.lst)|*.lst";
                        break;
                    }

                case 2:
                    {
                        SaveFile.Filter = "Parkam Hex Files (*.pkmhex)|*.pkmhex|Hex Files (*.hex)|*.hex";
                        break;
                    }

                case 3:
                    {
                        SaveFile.Filter = "Result Files (*.res)|*.res";
                        break;
                    }
            }

            Nullable<bool> MyDialog = SaveFile.ShowDialog();

            if (MyDialog == false)
            {
                MessageBox.Show("Error occured during saving file , ignoring save");
                return false;
            }

            Directory = SaveFile.FileName;
            string extension = System.IO.Path.GetExtension(SaveFile.FileName);

            FileStream save = new FileStream(Directory, FileMode.Create);
            
            if (extension.ToLowerInvariant() == ".txt" | extension.ToLowerInvariant() == ".asm" | extension.ToLowerInvariant() == ".pkm")
            {
                TextRange Content = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);
                Content.Save(save, DataFormats.Text.ToString());
            }
            else if (extension.ToLowerInvariant() == ".pkmlst")
            {
                TextRange Content = new TextRange(Result.Document.ContentStart, Result.Document.ContentEnd);
                Content.Save(save, DataFormats.Rtf.ToString());
            }
            else if (extension.ToLowerInvariant() == ".pkmhex")
            {
                StreamWriter Content = new StreamWriter(save);
                Content.Write(ParkamHexFileContent);
                Content.Close();
            }
            else if( extension.ToLowerInvariant() == ".lst")
            {
                TextRange Content = new TextRange(Result.Document.ContentStart, Result.Document.ContentEnd);
                Content.Save(save, DataFormats.Text.ToString());
            }
            else if( extension.ToLowerInvariant() == ".hex")
            {
                StreamWriter Content = new StreamWriter(save);
                Content.Write(HexFileContent);
                Content.Close();
            }
            else if (extension.ToLowerInvariant() == ".res")
            {
                StreamWriter Content = new StreamWriter(save);
                Content.Write(AnalyzerResult.Text);
                Content.Close();
            }
            else
            {
                MessageBox.Show("Unrecognized File Extension");
            }
    
            IsSaved = true;
            IsCreated = true;
            save.Close();

            return true;
        }

        public void RefreshSettings(Setting arg)
        {
            if (!arg.IsAutoCompleteEnabled & studio_setting.IsAutoCompleteEnabled)
            {
                AutoSense.Visibility = Visibility.Hidden;
            }

            if (!arg.IsAddressGenerationEnabled & studio_setting.IsAddressGenerationEnabled)
            {
                LineHelper.Document.Blocks.Clear();
            }

            studio_setting.Set(arg);
        }

        public void New(MessageBoxResult Result)
        {
            switch (Result)
            {
                case MessageBoxResult.Yes:
                    {
                        if (!IsCreated)
                        {
                            SaveAs(0);
                        }
                        else
                        {
                            Save();
                        }

                        this.Document.Blocks.Clear();
                        IsCreated = false;
                        IsSaved = false;
                        break;

                    }//MessageBoxResult.Yes ends

                case MessageBoxResult.No:
                    {
                        this.Document.Blocks.Clear();
                        IsCreated = false;
                        IsSaved = false;
                        break;
                    }
            }//switch result ends
        }

        public string Open()
        {
            Nullable<bool> MyDialog = OpenFile.ShowDialog();

            if (MyDialog == false)
            {
                MessageBox.Show("Error occured during opening file");
                return "";
            }

            thread_load = new Thread(Load);
            thread_load.Start();

            if (OpenFile.FileName.Contains(".asm") | OpenFile.FileName.Contains(".pkm") | OpenFile.FileName.Contains(".txt"))
            {
                IsSaved = true;
                IsCreated = true;
                Directory = OpenFile.FileName;
            } 

            return "Parkam Studio 1.0  (" + OpenFile.FileName.Substring(OpenFile.FileName.LastIndexOf("\\") + 1) + ")";
        }

        public byte[] GetHexa()
        {
            return hexa;
        }

        public ArrayList GetMemoryContent()
        {
            return MemoryContent;
        }

        //------------------------------------------------------------------>

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            /*if (e.Key == Key.Back)
            {
                //thread_check.Resume();
                AutoSense.Visibility = Visibility.Hidden;
            }
            else if (e.Key == Key.Enter | e.Key == Key.Space)
            {
                AutoSense.Visibility = Visibility.Hidden;
            }*/
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Up | e.Key == Key.Down)
            {
                if (AutoSense.IsVisible == true)
                {
                    AutoSense.Focus();
                }
            }
            else if (e.Key == Key.Left | e.Key == Key.Right | e.Key == Key.Enter)
            {
                AutoSense.Visibility = Visibility.Hidden;
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            MessageBox.Show("Double Click on Address Bar to change notation between decimal & hexadecimal");
        }

    };

    public partial class MainWindow : Window
    {

        public       MainWindow()
        {
            InitializeComponent();
            parkamCodeBox1.Initialize(ref lineHexAddress1, ref richTextBox1, ref parkamList1, ref parkamList2, ref parkamList3, ref _AutoSense, ref statusprogress, ref Status, ref Memory,ref Other, ref Result, ref Studio_Messages);
            lineHexAddress1.Initialize();
        }

        private void AutoComplete_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_AutoSense.SelectedItem != null)
            {
                string selection = _AutoSense.SelectedItem.ToString().ToLowerInvariant();
                parkamCodeBox1.AddAutoSense(selection);
            }
        }

        private void AutoComplete__PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_AutoSense.SelectedItem != null & e.Key == Key.Enter)
            {
                string selection = _AutoSense.SelectedItem.ToString().ToLowerInvariant();
                parkamCodeBox1.AddAutoSense(selection);
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (!parkamCodeBox1.IsSaved)
            {
                MessageBoxResult Result;

                if (parkamCodeBox1.IsCreated)
                {
                    Result = MessageBox.Show("Do you want to save changes to " + this.Title.Substring(this.Title.IndexOf('(') + 1, this.Title.Length - this.Title.IndexOf('(') - 3) + "?", "Parkam Studio 1.0", MessageBoxButton.YesNoCancel);
                }
                else
                {
                    Result = MessageBox.Show("Do you want to save changes to Untitled ?", "Parkam Studio 1.0", MessageBoxButton.YesNoCancel);
                }

                if (Result != MessageBoxResult.Cancel)
                {
                    parkamCodeBox1.New(Result);
                    this.Title = "Parkam Studio 1.0  (Untitled)";
                    tabItem1.Header = "MainTab";
                    Save.IsEnabled = false;
                    Simulate.IsEnabled = false;
                    Analyze.IsEnabled = false;
                    _Save.IsEnabled = false;
                    //enable save in Menu
                }
            }
            else
            {
                parkamCodeBox1.Document.Blocks.Clear();
                parkamCodeBox1.IsSaved   = false;
                parkamCodeBox1.IsCreated = false;

                this.Title = "Parkam Studio 1.0  (Untitled)";
                tabItem1.Header = "MainTab";
                Save.IsEnabled = false;
                Simulate.IsEnabled = false;
                Analyze.IsEnabled = false;
                _Save.IsEnabled = false;
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            string temp = parkamCodeBox1.Open();

            if (!String.IsNullOrWhiteSpace(temp))
            {
                this.Title = temp;

                if (temp.Contains(".asm") | temp.Contains(".pkm") | temp.Contains(".txt"))
                {
                    tabControl.SelectedIndex = 0;
                    tabItem1.Header = "MainTab";
                    //***************************
                    Save.IsEnabled = true;
                    _Save.IsEnabled = true;
                    Simulate.IsEnabled = false;
                    Analyze.IsEnabled = false;
                }
                else if (temp.Contains(".pkmlst"))
                {
                    tabControl.SelectedIndex = 1;
                }
                else if (temp.Contains(".pkmhex"))
                {
                    tabControl.SelectedIndex = 2;
                }
                else if (temp.Contains(".res"))
                {
                    tabControl.SelectedIndex = 3;
                }
                else
                {
                    tabControl.SelectedIndex = 4;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Save();
            tabItem1.Header = "MainTab";
        }

        private void Save_As_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl.SelectedIndex == 4)
            {
                MessageBox.Show("Nothing to save , change tab & try again");
            }
            else if (parkamCodeBox1.SaveAs(tabControl.SelectedIndex))
            {
                if (tabControl.SelectedIndex == 0)
                {
                    tabItem1.Header = "MainTab";
                    Save.IsEnabled = true;
                    _Save.IsEnabled = true;
                }
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Print();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Cut();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Copy();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Paste();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Redo();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Window2 my_box = new Window2();
            my_box.ShowDialog();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            Window3 my_window = new Window3(parkamCodeBox1.studio_setting);
            my_window.ShowDialog();
            parkamCodeBox1.RefreshSettings(my_window.mein);
        }

        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Analyze();
            tabControl.SelectedIndex = 3;
        }

        private void Assemble_Click(object sender, RoutedEventArgs e)
        {
            parkamCodeBox1.Assemble();
            tabControl.SelectedIndex = 1;
            tabItem1.Header = "MainTab";
            Simulate.IsEnabled = true;
            Analyze.IsEnabled = true;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            parkamCodeBox1.TextChanged -= new TextChangedEventHandler(TextChanged);
            parkamCodeBox1.Analyze_Text2();
            parkamCodeBox1.TextChanged += new TextChangedEventHandler(TextChanged);
            tabItem1.Header = "MainTab*";
        }

        private void Simulate_Click(object sender, RoutedEventArgs e)
        {
            Window1 SimulationWindow = new Window1(parkamCodeBox1.GetMemoryContent(), parkamCodeBox1.GetHexa());
            SimulationWindow.Show();
        }

        private void main_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            richTextBox1.ScrollToVerticalOffset(e.VerticalOffset);
            lineHexAddress1.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void Studio_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            parkamCodeBox1.Closing();
        }
    }
}
