using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Media;

namespace WpfApplication1
{
    public partial class Setting
    {
        public  bool  IsAutoCompleteEnabled       = true;
        public  bool  IsAddressGenerationEnabled  = true;
        public  bool  IsSyntaxHighLightingEnabled = true;

        public  Brush Brush1 = Brushes.Blue;
        public  Brush Brush2 = Brushes.Orange;
        public  Brush Brush3 = Brushes.Chartreuse;

        public  int    AutoCompleteDisplay = 5000;

        public  void Set(Setting arg)
        {
            this.IsAutoCompleteEnabled=arg.IsAutoCompleteEnabled;
            this.IsSyntaxHighLightingEnabled=arg.IsSyntaxHighLightingEnabled;
            this.IsAddressGenerationEnabled=arg.IsAddressGenerationEnabled;

            this.Brush1=arg.Brush1;
            this.Brush2=arg.Brush2;
            this.Brush3=arg.Brush3;

            this.AutoCompleteDisplay=arg.AutoCompleteDisplay;
        }

    }

    class Info
    {
        public int     PC_Content      = 0;
        public int     Address_Content = 0;

        public bool    C_Flag = false;
        public bool    O_Flag = false;
        public bool    N_Flag = false;
        public bool    Z_Flag = false;

        public byte    A_Content     = 0;
        public byte    B_Content     = 0;
        public byte    X_Content     = 0;

        public byte    In_Content    = 0;
        public byte    Out_Content   = 0;
        
        public byte    IR_Content    = 0;
        public byte    HTemp_Content = 0;
        public byte    LTemp_Content = 0;

        public byte    Data_Content  = 0;
        public byte [] RAM_Content   =new byte[4096];

        public Info    ()             
        {
            for (int i = 0; i < 4096 ; i++)
            {
                RAM_Content[i] = 0;
            }
        }

        public Info    (byte[] hex)   
        {
            for (int i = 0; i < 4096; i++)
            {
                this.RAM_Content[i] = hex[i];
            }

            Data_Content = RAM_Content[0];
        }

        public Info    (Info  input)  
        {
            PC_Content=input.PC_Content;
            Address_Content = input.Address_Content;

            C_Flag = input.C_Flag;
            O_Flag = input.O_Flag;
            N_Flag = input.N_Flag;
            Z_Flag = input.Z_Flag;

            A_Content = input.A_Content;
            B_Content = input.B_Content;
            X_Content = input.X_Content;
            In_Content = input.In_Content;
            Out_Content = input.Out_Content;
            Data_Content = input.Data_Content;

            IR_Content = input.IR_Content;
            HTemp_Content = input.HTemp_Content;
            LTemp_Content = input.LTemp_Content;

            for (int i = 0; i < 4096; i++)
            {
                this.RAM_Content[i] = input.RAM_Content[i];
            }
        }
    }

    class AnalyzeResult
    {
        public int  NumberOfINA;
        public int  NumberOfINS;
        public int  NumberOfCycles;
        public byte RAM_Content;
        public Info Memory;

        public AnalyzeResult(int arg1, int arg2 , int arg3 , Info arg4)
        {
            NumberOfINA = arg1;
            NumberOfCycles = arg2;
            NumberOfINS = arg3;
            Memory = arg4;
        }
    }

    class TimeAnalyzer
    {   
        private int   NumberOfINA    = 0;
        private int   NumberOfINS    = 0;
        private int   NumberOfCycles = 0;
        private Queue InputValues    = new Queue();

        public AnalyzeResult Analyze(Queue InputValues , Info Data , int destination_address = 4095)
        {
            NumberOfINA = 0;
            Data.PC_Content = 0;

            Perform_F_Fetch(Data);

            while (true)
            {
                
                switch (Data.Data_Content)
                {
                    case 16:
                        {//ADD
                            Perform_Add(Data);
                            break;
                        }

                    case 17:
                        {//SUB
                            Perform_Sub(Data);
                            break;
                        }

                    case 18:
                        {//CMP
                            Perform_Cmp(Data);
                            break;
                        }

                    case 19:
                        {//DEC
                            Perform_Dec(Data);
                            break;
                        }

                    case 20:
                        {//AND
                            Perform_And(Data);
                            break;
                        }

                    case 21:
                        {//OR
                            Perform_Or(Data);
                            break;
                        }

                    case 22:
                        {//XOR
                            Perform_Xor(Data);
                            break;
                        }

                    case 23:
                        {//CPTA
                            Perform_Cpta(Data);
                            break;
                        }

                    case 24:
                        {//RORA
                            Perform_Rora(Data);
                            break;
                        }

                    case 25:
                        {//ROLA
                            Perform_Rola(Data);
                            break;
                        }

                    case 26:
                        {//RORC
                            Perform_Rorc(Data);
                            break;
                        }

                    case 27:
                        {//ROLC
                            Perform_Rolc(Data);
                            break;
                        }

                    case 28:
                        {//CLC
                            Perform_Clc(Data);
                            break;
                        }

                    case 29:
                        {//CPTC
                            Perform_Cptc(Data);
                            break;
                        }

                    case 30:
                        {//INC
                            Perform_Inc(Data);
                            break;
                        }

                    case 31:
                        {//HLT
                            return new AnalyzeResult(NumberOfINA, NumberOfCycles, NumberOfINS, Data);
                            break;
                        }

                    case 32:
                        {//PMOV A,B
                            Perform_Pmov_AB(Data);
                            break;
                        }

                    case 33:
                        {//PMOV B,A
                            Perform_Pmov_BA(Data);
                            break;
                        }

                    case 34:
                        {//PMOV A,X
                            Perform_Pmov_AX(Data);
                            break;
                        }

                    case 35:
                        {//PMOV X,A 
                            Perform_Pmov_XA(Data);
                            break;
                        }

                    case 36:
                    case 00:
                        {//NOOP
                            break;
                        }

                    case 37:
                        {//SWP
                            Perform_Swp(Data);
                            break;
                        }

                    case 38:
                        {//INA
                            if (InputValues.Count == 0)
                            {
                                Perform_Ina(0 , Data);
                                NumberOfINA++;
                            }
                            else
                            {
                                try
                                {
                                    Perform_Ina((byte)InputValues.Dequeue() , Data);
                                }
                                catch
                                {
                                    Perform_Ina(0 , Data);
                                }
                            }

                            break;
                        }

                    case 39:
                        {//OUTA
                            Perform_Outa(Data);
                            break;
                        }

                    case 96:
                        {//PMVI A
                            Perform_Pmvi_A(Data);
                            break;
                        }

                    case 112:
                        {//PMVI X
                            Perform_Pmvi_X(Data);
                            break;
                        }

                    case 64:
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
                        {//LDA
                            Perform_Lda(Data);
                            break;
                        }

                    case 80:
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
                        {//STA
                            Perform_Sta(Data);
                            break;
                        }

                    case 128:
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
                        {//JMP
                            Perform_Jump(true , Data);
                            break;
                        }

                    case 144:
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
                        {//JZR
                            Perform_Jump(Data.Z_Flag, Data);
                            break;
                        }

                    case 160:
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
                        {//JNZ
                            Perform_Jump(!Data.Z_Flag , Data);
                            break;
                        }

                    case 176:
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
                        {//JCY
                            Perform_Jump(Data.C_Flag, Data);
                            break;
                        }

                    case 192:
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
                        {//JNC
                            Perform_Jump(!Data.C_Flag, Data);
                            break;
                        }

                    case 208:
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
                        {//JNG
                            Perform_Jump(Data.N_Flag, Data);
                            break;
                        }

                    case 224:
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
                        {//JPS
                            Perform_Jump(!Data.N_Flag, Data);
                            break;
                        }

                    case 240:
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
                        {//JOV
                            Perform_Jump(Data.O_Flag, Data);
                            break;
                        }
                }

                NumberOfINS++;

                if (!Perform_Fetch(Data, destination_address))
                {
                    break;
                }

                if (NumberOfINS > 100000)
                {
                    return new AnalyzeResult(NumberOfINA , NumberOfCycles , -1 , Data );
                }
            }

            return new AnalyzeResult(NumberOfINA,NumberOfCycles,NumberOfINS,Data);

        }

        private void Perform_F_Fetch(Info Data)
        {
            Data.IR_Content = Data.Data_Content = Data.RAM_Content[0];
            Data.PC_Content = 1;
            Data.Address_Content = Data.PC_Content;
            NumberOfCycles += 6;
        }

        private bool Perform_Fetch  (Info Data , int destination_address)
        {
            bool result=true;

            Data.IR_Content = Data.Data_Content = Data.RAM_Content[Data.Address_Content];

            if (Data.Address_Content == destination_address)
            {
                result = false;
            }

            Data.PC_Content++;
            Data.Address_Content = Data.PC_Content;
            NumberOfCycles += 5;

            return result;
        }

        private void Perform_Add    (Info Data)
        {
            int temp = Data.A_Content + Data.B_Content;

            Data.O_Flag = false;

            //************************************************************

            if (Data.A_Content < 128 && Data.B_Content < 128)     //(+)plus(+)-->(-)
            {
                if (127 < temp && temp < 256)
                {
                    Data.O_Flag = true;
                }
            }
            else if (Data.A_Content > 127 && Data.B_Content > 127)//(-)plus(-)-->(+)
            {
                if (255 < temp && temp < 384)
                {
                    Data.O_Flag = true;
                }
            }

            //************************************************************

            if (temp < 256)
            {
                Data.C_Flag = false;
                Data.A_Content = (byte)temp;
            }
            else
            {
                Data.C_Flag = true;
                Data.A_Content = (byte)(temp-256);
            }

            //************************************************************

            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            NumberOfCycles += 3;

            return;
        }

        private void Perform_Sub    (Info Data)
        {
            int B_Content_Complement = 256 - Data.B_Content;
            int temp = Data.A_Content + B_Content_Complement;

            Data.O_Flag = false;

            //************************************************************

            if      (Data.A_Content < 128 && Data.B_Content > 127)//(+)minus(-)-->(-)
            {
                if (127 < temp && temp < 256)
                {
                    Data.O_Flag = true;
                }
            }
            else if (Data.A_Content > 127 && Data.B_Content < 128)//(-)minus(+)-->(+)
            {
                if (255 < temp && temp < 384)
                {
                    Data.O_Flag = true;
                }
            }

            //************************************************************

            if (temp < 256)
            {
                Data.C_Flag = true;
                Data.A_Content = (byte)temp;
            }
            else
            {
                Data.C_Flag = false;
                Data.A_Content = (byte)(temp - 256);
            }

            //************************************************************

            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            NumberOfCycles += 3;

            return;
        }

        private void Perform_Cmp    (Info Data)
        {
            int A_Content_Temp = Data.A_Content;
            int B_Content_Complement = 256 - Data.B_Content;
            int temp = Data.A_Content + B_Content_Complement;

            Data.O_Flag = false;

            //************************************************************

            if (Data.A_Content < 128 && Data.B_Content > 127)//(+)minus(-)-->(-)
            {
                if (127 < temp && temp < 256)
                {
                    Data.O_Flag = true;
                }
            }
            else if (Data.A_Content > 127 && Data.B_Content < 128)//(-)minus(+)-->(+)
            {
                if (255 < temp && temp < 384)
                {
                    Data.O_Flag = true;
                }
            }

            //************************************************************

            if (temp < 256)
            {
                A_Content_Temp = temp;
            }
            else
            {
                A_Content_Temp = temp - 256;
            }

            //************************************************************

            Data.Z_Flag = (A_Content_Temp == 0) ? true : false;
            Data.N_Flag = (A_Content_Temp > 127) ? true : false;
            NumberOfCycles += 3;

            return;
        }

        private void Perform_Dec    (Info Data)
        {
            if (Data.A_Content == 128)//A is smallest negative number so decreasing it causes overflow
            {
                Data.O_Flag = true;//sum of two negative number causes overflow
            }
            else
            {
                Data.O_Flag = false;
            }

            if (Data.A_Content == 0)
            {
                Data.A_Content = 255;
            }
            else
            {
                Data.A_Content = (byte)(Data.A_Content - 1);
            }

          
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            NumberOfCycles += 3;

            return;
        }

        private void Perform_And    (Info Data)
        {
            Data.A_Content = (byte)(Data.A_Content & Data.B_Content);
            Data.O_Flag = false;
            
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            NumberOfCycles += 3;

            return;
        }

        private void Perform_Or     (Info Data)
        {
            Data.A_Content = (byte)(Data.A_Content | Data.B_Content);
            Data.O_Flag = false;
            
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            NumberOfCycles += 3;

            return;
        }

        private void Perform_Xor    (Info Data)
        {
            Data.A_Content = (byte)(Data.A_Content ^ Data.B_Content);
            Data.O_Flag = false;
            
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            NumberOfCycles += 3;

            return;
        }

        private void Perform_Cpta   (Info Data)
        {
            Data.A_Content = (byte)(255 - Data.A_Content);
            Data.O_Flag = false;
           
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            NumberOfCycles += 3;

            return;
        }

        private void Perform_Rora   (Info Data)
        {
            byte A_Content_Temp = (byte)(Data.A_Content / 2);

            if ((Data.A_Content % 2) == 1)
            {
                Data.C_Flag = true;
                A_Content_Temp += 128;
            }
            else
            {
                Data.C_Flag = false;
            }

            Data.A_Content = A_Content_Temp;
            Data.O_Flag = false;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;

            NumberOfCycles += 3;
            return;
        }

        private void Perform_Rola   (Info Data)
        {
            int A_Content_Temp = Data.A_Content * 2;

            if (A_Content_Temp > 255)
            {
                Data.C_Flag = true;
                A_Content_Temp ++;
                Data.A_Content = (byte)(A_Content_Temp - 256);
            }
            else
            {
                Data.C_Flag = false;
                Data.A_Content = (byte)A_Content_Temp;
            }

            Data.O_Flag = false;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;

            NumberOfCycles += 3;
            return;
        }

        private void Perform_Rorc   (Info Data)
        {
            int A_Content_Temp;

            if (Data.C_Flag)
            {
                A_Content_Temp = Data.A_Content + 256;
            }
            else
            {
                A_Content_Temp = Data.A_Content;
            }

            if ((Data.A_Content % 2) == 1)
            {
                Data.C_Flag = true;
            }
            else
            {
                Data.C_Flag = false;
            }

            Data.A_Content = (byte)(A_Content_Temp / 2);
            Data.O_Flag = false;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            
            NumberOfCycles += 3;
            return;
        }

        private void Perform_Rolc   (Info Data)
        {
            int A_Content_Temp = Data.A_Content * 2;

            if (Data.C_Flag)
            {
                A_Content_Temp++;
            }

            if (A_Content_Temp > 255)
            {
                Data.C_Flag = true;
                Data.A_Content = (byte)(A_Content_Temp - 256);
            }
            else
            {
                Data.C_Flag = false;
                Data.A_Content = (byte)A_Content_Temp;
            }
            Data.O_Flag = false;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;

            NumberOfCycles += 3;
            return;
        }

        private void Perform_Clc    (Info Data)
        {
            Data.C_Flag = false;

            NumberOfCycles += 2;
            return;
        }

        private void Perform_Cptc   (Info Data)
        {
            Data.C_Flag = (Data.C_Flag) ? false : true;

            NumberOfCycles += 2;
            return;
        }

        private void Perform_Inc    (Info Data)
        {
            if (Data.A_Content == 127)//A is biggest positive number so increasing it causes overflow
            {
                Data.O_Flag = true;//sum of two positive number causes overflow
            }
            else
            {
                Data.O_Flag = false;
            }

            if (Data.A_Content == 255)
            {
                Data.A_Content = 0;
            }
            else
            {
                Data.A_Content = (byte)(Data.A_Content + 1);
            }

            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            
            NumberOfCycles += 3;
            return;
        }

        private void Perform_Pmov_AB(Info Data)
        {
            Data.A_Content = Data.B_Content;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;

            NumberOfCycles += 2;
            return;
        }

        private void Perform_Pmov_BA(Info Data)
        {
            Data.B_Content = Data.A_Content;
            
            NumberOfCycles += 2;
            return;
        }

        private void Perform_Pmov_AX(Info Data)
        {
            Data.A_Content = Data.X_Content;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            
            NumberOfCycles += 2;
            return;
        }

        private void Perform_Pmov_XA(Info Data)
        {
            Data.X_Content = Data.A_Content;

            NumberOfCycles += 2;
            return;
        }

        private void Perform_Swp    (Info Data)
        {
            byte A_Content_Temp = Data.A_Content;

            Data.A_Content = Data.B_Content;
            Data.B_Content = A_Content_Temp;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;

            NumberOfCycles += 2;
            return;
        }

        private void Perform_Ina    (byte arg ,Info Data)
        {
            Data.A_Content = arg;
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            
            NumberOfCycles += 2;
            return;
        }

        private void Perform_Outa   (Info Data)
        {
            Data.Out_Content = Data.A_Content;
           
            NumberOfCycles += 2;
            return;
        }

        private void Perform_Lda    (Info Data)
        {
            Data.HTemp_Content = (byte)(Data.IR_Content % 16);
            Data.LTemp_Content = Data.RAM_Content[Data.Address_Content];
            Data.A_Content = Data.RAM_Content[(Data.HTemp_Content * 256) + Data.LTemp_Content];
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            Data.PC_Content++;
            Data.Address_Content = Data.PC_Content;

            NumberOfCycles += 6;
            return;
        }

        private void Perform_Sta    (Info Data)
        {
            Data.HTemp_Content = (byte)(Data.IR_Content % 16);
            Data.LTemp_Content = Data.RAM_Content[Data.Address_Content ];
            Data.RAM_Content[(Data.HTemp_Content * 256) + Data.LTemp_Content] = Data.A_Content;
            Data.PC_Content++;
            Data.Address_Content = Data.PC_Content;

            NumberOfCycles += 6;
            return;
        }

        private void Perform_Pmvi_A (Info Data)
        {
            Data.A_Content = Data.RAM_Content[Data.PC_Content];
            Data.N_Flag = (Data.A_Content > 127) ? true : false;
            Data.Z_Flag = (Data.A_Content == 0) ? true : false;
            Data.PC_Content++;
            Data.Address_Content = Data.PC_Content;
            NumberOfCycles += 3;
            return;
        }

        private void Perform_Pmvi_X (Info Data)
        {
            Data.X_Content = Data.RAM_Content[Data.PC_Content];
            Data.PC_Content++;
            Data.Address_Content = Data.PC_Content;
            
            NumberOfCycles += 3;
            return;
        }

        private void Perform_Jump   (bool arg1,Info Data)
        {
            if (arg1)
            {
                Data.HTemp_Content = (byte)(Data.IR_Content % 16);//(Data.RAM_Content[Data.IR_Content ] % 16);
                Data.LTemp_Content = Data.RAM_Content[Data.Address_Content ];
                Data.PC_Content = (Data.HTemp_Content * 256) + Data.LTemp_Content;
                Data.Address_Content = Data.PC_Content;
                NumberOfCycles += 4;
            }
            else 
            {
                Data.PC_Content += 1;
                Data.Address_Content = Data.PC_Content;
                NumberOfCycles += 2;
            }

            return;
        }
    }

}
