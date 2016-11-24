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
using System.Threading;

namespace Parkam_Studio
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Parkam_Studio"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Parkam_Studio;assembly=Parkam_Studio"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:_8BitRegister/>
    ///
    /// </summary>
    public class DataRegister : TextBlock
    {
        //static DataRegister()
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(DataRegister), new FrameworkPropertyMetadata(typeof(DataRegister)));
        //}

        private byte      Content;
        private Brush     Brush;
        private TextBlock Control;
        private Thread thread_blink;
        
        private void Blink()
        {
            bool state = true;

            while (true)
            {
                if (!state)
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brush  ));
                }
                else
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brushes.White));
                }

                state = !state;
                Thread.Sleep(500);
            }
        }

        public      DataRegister()               
        {
            Content = 0;
        }

        public byte Get       ()                 
        {
            return Content;
        }

        public void Set(byte value)              
        {
            Content = value;
        }

        public void Refresh   ()                 
        {
            this.Text = Content.ToString("X2");
        }

        public void Refresh(byte value)          
        {
            Set(value);
            Refresh();
        }

        public void SetBrush(Brush Brush)
        {
            this.Brush = Brush;
        }

        public void SetControl(ref TextBlock Control)
        {
            this.Control = Control;
        }

        public void Clock_Show()                 
        {
            Control.Foreground = Brushes.White;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Clock_Hide()                 
        {
            Control.Foreground = Brush;

            if (thread_blink!=null)
            {
                thread_blink.Abort();
            }

            this.Foreground = Brush;
        }

        public void Unknown   ()                 
        {
            this.Text = "XX";
        }

        public void Close()
        {
            if (thread_blink != null)
            {
                thread_blink.Abort();
            }
        }
  
    };

    public class HighTempRegister : TextBlock
    {
        private byte      Content;
        private Brush     Brush;
        private TextBlock Control;
        private Thread    thread_blink;

        private void Blink()
        {
            bool state = true;

            while (true)
            {
                if (!state)
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brush));
                }
                else
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brushes.White));
                }

                state = !state;
                Thread.Sleep(500);
            }
        }

        public HighTempRegister()                
        {
            Content = 0;
        }

        public byte Get       ()                 
        {
            return Content;
        }

        public void Set(byte value)              
        {
            if(value <= 16)
            {
                Content = value;
            }
            else
            {
                MessageBox.Show("Error occured while setting HTemp"); 
            }
        }

        public void Refresh   ()                 
        {
            this.Text = Content.ToString("X2");
        }

        public void Refresh(byte value)          
        {
            Set(value);
            Refresh();
        }

        public void SetBrush(Brush Brush)        
        {
            this.Brush = Brush;
        }

        public void SetControl(ref TextBlock Control)
        {
            this.Control = Control;
        }

        public void Clock_Show()                 
        {
            Control.Foreground = Brushes.White;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Clock_Hide()                 
        {
            Control.Foreground = Brush;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            this.Foreground = Brush;
        }

        public void Close()
        {
            if (thread_blink != null)
            {
                thread_blink.Abort();
            }
        }

    };

    public class AddressRegister : TextBlock
    {
        private int       Content;
        private Brush     Brush;
        private TextBlock Control;
        private Thread thread_blink;

        private void Blink()
        {
            bool state = true;

            while (true)
            {
                if (!state)
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brush));
                }
                else
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brushes.White));
                }

                state = !state;
                Thread.Sleep(500);
            }
        }

        public AddressRegister()                     
        {
            Content = 0;
        }

        public int  Get       ()                     
        {
            return Content;
        }

        public void Set(int value)                   
        {
            Content = value;
        }

        public void Refresh   ()                     
        {
            this.Text = Content.ToString("X3");
        }

        public void Refresh   (int value)            
        {
            Set(value);
            Refresh();
        }

        public void SetBrush  (Brush Brush)          
        {
            this.Brush = Brush;
        }

        public void SetControl(ref TextBlock Control)
        {
            this.Control = Control;
        }

        public void Clock_Show()                     
        {
            this.Control.Foreground = Brushes.White;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Clock_Hide()                     
        {
            this.Control.Foreground = Brush;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            this.Foreground = Brush;
        }

        public void Close()
        {
            if (thread_blink != null)
            {
                thread_blink.Abort();
            }
        }
  
    };

    public class ProgramCounter : TextBlock
    {
        private static string Default = "inc_PC";

        private int       Content;
        private Brush     Brush;
        private TextBlock Control;
        private Thread    thread_blink;

        private void Blink()
        {
            bool state = true;

            while (true)
            {
                if (!state)
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brush));
                }
                else
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brushes.White));
                }

                state = !state;
                Thread.Sleep(500);
            }
        }
        
        public ProgramCounter()                  
        {
            Content = 0;
        }

        public int  Get      ()                  
        {
            return Content;
        }

        public void Set(int value)               
        {
            Content = value;
        }

        public void Reset    ()                  
        {
            Content = 0;
        }

        public int  Increment()                  
        {
            if (Content != 4095)
            {
                Content++;
            }
            else
            {
                Content = 0;
                MessageBox.Show("Overflow occured while incrementing program counter");
            }

            return Content;
        }

        public void Refresh  ()                  
        {
            this.Text = Content.ToString("X3");
        }
        
        public void Refresh (int value          )
        {
            Set(value);
            Refresh();
        }

        public void SetBrush(Brush Brush)        
        {
            this.Brush = Brush;
        }

        public void SetControl(ref TextBlock Control)
        {
            this.Control = Control;
        }

        public void Inc_Show  ()                 
        {
            Control.Foreground = Brushes.White;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Inc_Hide  ()                 
        {
            Control.Foreground = Brush;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }
            
            this.Foreground = Brush;
        }

        public void Clock_Show()                 
        {
            Control.Text = "clk_PC";
            Control.Foreground = Brushes.White;
            
            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Clock_Hide()                 
        {
            Control.Foreground = Brush;
            Control.Text = Default;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            this.Foreground = Brush;
        }

        public void Close()
        {
            if (thread_blink != null)
            {
                thread_blink.Abort();
            }
        }

        public void Reset_Show()                 
        {
            Control.Text = "rst";
            Control.Foreground = Brushes.White;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Reset_Hide()                 
        {
            Control.Foreground = Brush;
            Control.Text = Default;

            if (thread_blink != null )
            {
                thread_blink.Abort();
            }

            this.Foreground = Brush;
        }

    };

    public class InformationText : TextBlock
    {
        private string Current;
        private string Next;
        private string Message;
        private TextBlock CurrentIns;
        private TextBlock NextIns;

        public void SetControl(ref TextBlock arg1, ref TextBlock arg2)
        {
            CurrentIns = arg1;
            NextIns = arg2;
        }
             
        public void SetCurrent  (string Current)              
        {
            this.Current = Current;
            Default();
        }

        public void SetNext     (string Next)                 
        {
            this.Next = Next;
            Default();
        }

        public void SetMessage(string Message)
        {
            this.Text = Message;
            Default();
        }

        public void SetState    (string Current , string Next)
        {
            this.Current = Current;
            this.Next = Next;
            this.Message = "---";
            Default();
        }

        private void Default     ()                            
        {
            this.CurrentIns.Text = Current;
            this.NextIns.Text = Next;
        }
    };

    public class Flag : TextBlock
    {
        private   bool      Status;
        protected TextBlock Control;
        protected Brush     Brush;
        protected Thread   thread_blink;

        public void Blink()
        {
            bool state = true;

            while (true)
            {
                if (!state)
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brush));
                }
                else
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brushes.White));
                }

                state = !state;
                Thread.Sleep(500);
            }
        }

        public bool Get     ()           
        {
            return this.Status;
        }

        public void Set     (bool Status)
        {
            this.Status = Status;
        }

        public void Refresh ()           
        {
            this.Text = (Status ? "1" : "0");
        }

        public void Refresh (bool Status)
        {
            Set(Status);
            Refresh();
        }

        public void SetBrush(Brush Brush)
        {
            this.Brush = Brush;
        }

        public void SetControl(ref TextBlock Control)
        {
            this.Control = Control;
        }

        public void Clock_Show         ()
        {
            this.Control.Foreground = Brushes.White;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Clock_Hide         ()
        {
            this.Control.Foreground = Brush;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            this.Foreground = Brush;
        }

        public void Close()
        {
            if (thread_blink != null)
            {
                thread_blink.Abort();
            }
        }

    };

    public class G_Flag : Flag
    {
        private string Default;

        public void Reset()
        {
            Set(false);
        }

        public void Reset_Show()
        {
            Default = Control.Text;
            Control.Text = "rst";
            Control.Foreground = Brushes.White;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Reset_Hide()
        {
            Control.Foreground = Brush;
            Control.Text       = Default;

            if (thread_blink != null )
            {
                thread_blink.Abort();
            }

            this.Foreground = Brush;
        }

    };

    public class InsructionDecoder : TextBlock
    {
        private byte      input;
        private string    output;
        private Brush     Brush;
        private TextBlock Control;
        private Thread    thread_blink;

        public void Set(byte address, bool N_Flag , bool Z_Flag , bool O_Flag , bool C_Flag)
        {
            input = address;
            output = GetMappedAddress(input, N_Flag, Z_Flag, O_Flag, C_Flag);
            this.Text = input.ToString("X2") + "\n" + output;
        }

        public byte   GetInput ()
        {
            return input;
        }

        public string GetOutput()
        {
            return output;
        }

        public void Clock_Show()
        {
            Control.Foreground = Brushes.White;
            
            if (thread_blink != null)
            {
                thread_blink.Abort();
            }
            thread_blink = new Thread(Blink);
            thread_blink.Start();
        }

        public void Clock_Hide()
        {
            Control.Foreground = Brush;

            if (thread_blink != null)
            {
                thread_blink.Abort();
            }

            this.Foreground = Brush;
        }

        public void Close()
        {
            if (thread_blink != null)
            {
                thread_blink.Abort();
            }
        }

        public void SetBrush(Brush Brush)
        {
            this.Brush = Brush;
        }

        public void SetControl(ref TextBlock Control)
        {
            this.Control = Control;
        }

        private void Blink()
        {
            bool state = true;

            while (true)
            {
                if (!state)
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brush));
                }
                else
                {
                    Dispatcher.Invoke(new System.Action(() => this.Foreground = Brushes.White));
                }

                state = !state;
                Thread.Sleep(500);
            }
        }

        private string GetMappedAddress(byte address,bool N_Flag , bool Z_Flag , bool O_Flag , bool C_Flag)
        {
            switch (address)
            {
                case 16:
                    {
                        return "068";
                    }

                case 17:
                    {
                        return "074";
                    }

                case 18:
                    {
                        return "080";
                    }

                case 19:
                    {
                        return "08C";
                    }

                case 20:
                    {
                        return "098";
                    }

                case 21:
                    {
                        return "0A4";
                    }

                case 22:
                    {
                        return "0B0";
                    }

                case 23:
                    {
                        return "0BC";
                    }

                case 24:
                    {
                        return "0C8";
                    }

                case 25:
                    {
                        return "0D4";
                    }

                case 26:
                    {
                        return "0E0";
                    }

                case 27:
                    {
                        return "0EC";
                    }

                case 28:
                    {
                        return "0F8";
                    }

                case 29:
                    {
                        return "104";
                    }

                case 30:
                    {
                        return "110";
                    }

                case 31:
                    {
                        return "Halted";
                    }

                //--------------------
                case 32:
                    {
                        return "008";
                    }

                case 33:
                    {
                        return "014";
                    }

                case 34:
                    {
                        return "020";
                    }

                case 35:
                    {
                        return "02C";
                    }

                case 00:
                case 36:
                    {
                        return "038";
                    }

                case 37:
                    {
                        return "044";
                    }

                case 38:
                    {
                        return "050";
                    }

                case 39:
                    {
                        return "05C";
                    }

                //-------------------

                case 96: //PMVI A
                    {
                        return "13C";
                    }

                case 112://PMVI X
                    {
                        return "14C";
                    }

                case 64://LDA
                case 65:
                case 66:
                case 67:
                case 68:
                case 69:
                case 70:
                case 71:
                case 72:
                case 73:
                case 74:
                case 75:
                case 76:
                case 77:
                case 78:
                case 79:
                    {
                        return "11C";
                    }

                case 80://STA
                case 81:
                case 82:
                case 83:
                case 84:
                case 85:
                case 86:
                case 87:
                case 88:
                case 89:
                case 90:
                case 91:
                case 92:
                case 93:
                case 94:
                case 95:
                    {
                        return "12C";
                    }

                case 128://JMP
                case 129:
                case 130:
                case 131:
                case 132:
                case 133:
                case 134:
                case 135:
                case 136:
                case 137:
                case 138:
                case 139:
                case 140:
                case 141:
                case 142:
                case 143:
                    {
                        return "15C";
                    }

                case 144://JZR
                case 145:
                case 146:
                case 147:
                case 148:
                case 149:
                case 150:
                case 151:
                case 152:
                case 153:
                case 154:
                case 155:
                case 156:
                case 157:
                case 158:
                case 159:
                    {
                        if (Z_Flag)
                        {
                            return "15C";
                        }
                        else
                        {
                            return "16C";
                        }
                    }

                case 160://JNZ
                case 161:
                case 162:
                case 163:
                case 164:
                case 165:
                case 166:
                case 167:
                case 168:
                case 169:
                case 170:
                case 171:
                case 172:
                case 173:
                case 174:
                case 175:
                    {
                        if (!Z_Flag)
                        {
                            return "15C";
                        }
                        else
                        {
                            return "16C";
                        }
                    }

                case 176://JCY
                case 177:
                case 178:
                case 179:
                case 180:
                case 181:
                case 182:
                case 183:
                case 184:
                case 185:
                case 186:
                case 187:
                case 188:
                case 189:
                case 190:
                case 191:
                    {
                        if (C_Flag)
                        {
                            return "15C";
                        }
                        else
                        {
                            return "16C";
                        }
                    }

                case 192://JNC
                case 193:
                case 194:
                case 195:
                case 196:
                case 197:
                case 198:
                case 199:
                case 200:
                case 201:
                case 202:
                case 203:
                case 204:
                case 205:
                case 206:
                case 207:
                    {
                        if (!C_Flag)
                        {
                            return "15C";
                        }
                        else
                        {
                            return "16C";
                        }
                    }

                case 208://JNG
                case 209:
                case 210:
                case 211:
                case 212:
                case 213:
                case 214:
                case 215:
                case 216:
                case 217:
                case 218:
                case 219:
                case 220:
                case 221:
                case 222:
                case 223:
                    {
                        if (N_Flag)
                        {
                            return "15C";
                        }
                        else
                        {
                            return "16C";
                        }
                    }

                case 224://JPS
                case 225:
                case 226:
                case 227:
                case 228:
                case 229:
                case 230:
                case 231:
                case 232:
                case 233:
                case 234:
                case 235:
                case 236:
                case 237:
                case 238:
                case 239:
                    {
                        if (!N_Flag)
                        {
                            return "15C";
                        }
                        else
                        {
                            return "16C";
                        }
                    }

                case 240://JOV
                case 241:
                case 242:
                case 243:
                case 244:
                case 245:
                case 246:
                case 247:
                case 248:
                case 249:
                case 250:
                case 251:
                case 252:
                case 253:
                case 254:
                case 255:
                    {
                        if (O_Flag)
                        {
                            return "15C";
                        }
                        else
                        {
                            return "16C";
                        }
                    }

                default:
                    {
                        return "Error Occured";
                    }
            }
        }



    }

}