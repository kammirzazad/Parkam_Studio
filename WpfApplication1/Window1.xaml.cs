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
using System.Windows.Shapes;
using System.Collections;
using System.Windows.Navigation;
using System.Threading;
using System.IO;
using System.Globalization;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 

    public partial class Window1 : Window
    {
        //Boolean
        bool returntofirst  = false;
        bool Skip           = false;
        bool reset          = false;
        bool animation_on   = true;
        bool move_requested = false;
        
        //Intigers
        int timing  = 5000;
        int Tcycle  = 0;
        int Tselect = 0;
        int Iselect = 0;
        int start_address = 0;

        //Byte-------------------
        byte   In_Content = 0;
        byte[] Ablauf;
        byte[] Ablauf_Backup;
        char[] trim4 = { ' ', ',' };

        ///Transforms
        TranslateTransform move1;  //for Data_Bus_Content
        TranslateTransform move2;  //for Address_Bus_Content
        TranslateTransform move3;  //for Register_Output_Content
        TranslateTransform move4;  //for Register_Input_Content

        //Threads
        System.Threading.Thread simulation;
       
        //Info
        Info   Last_Info;
        Info[] Sim_Info;
        ArrayList Description_Backup;

        public Window1()
        {
            InitializeComponent();
            MessageBox.Show("Error Has occured , constructor has been called with no arguements");
        }

        public Window1(ArrayList Description, byte[] hexa)
        {
            InitializeComponent();

            //---------------Orient ProgresssBars--------------

            ScaleTransform mirror = new ScaleTransform(1, -1);

            PC_In.RenderTransform = mirror;
            MUX_1.RenderTransform = mirror;
            ALU_B.RenderTransform = mirror;
            IR_Input.RenderTransform = mirror;
            A_Input_1.RenderTransform = mirror;
            B_Input_1.RenderTransform = mirror;
            X_Input_1.RenderTransform = mirror;
            In_Port_1.RenderTransform = mirror;
            IR_Output.RenderTransform = mirror;
            HTemp_In_1.RenderTransform = mirror;
            LTemp_Input.RenderTransform = mirror;
            Data_Bus_WR_2.RenderTransform = mirror;
            Decoder_Output.RenderTransform = mirror;

            //--------------Set Dependent Controls & Brushes--------------------

            A_Register.SetControl(ref A_Control);
            A_Register.SetBrush(Brushes.Violet);
            B_Register.SetControl(ref B_Control);
            B_Register.SetBrush(Brushes.Violet);
            X_Register.SetControl(ref X_Control);
            X_Register.SetBrush(Brushes.Violet);
            Output_Register.SetControl(ref Output_Control);
            Output_Register.SetBrush(Brushes.Violet);

            IR_Register.SetControl(ref IR_Control);
            IR_Register.SetBrush(Brushes.Blue);
            PC_Register.SetControl(ref PC_Control);
            PC_Register.SetBrush(Brushes.Blue);
            LTemp_Register.SetControl(ref LTemp_Control);
            LTemp_Register.SetBrush(Brushes.Blue);
            HTemp_Register.SetControl(ref HTemp_Control);
            HTemp_Register.SetBrush(Brushes.Blue);

            Q_Register.SetControl(ref Q_Control);
            Q_Register.SetBrush(Brushes.Plum);
            Data_Register.SetControl(ref Data_Control);
            Data_Register.SetBrush(Brushes.Plum);
            Address_Register.SetControl(ref Address_Control);
            Address_Register.SetBrush(Brushes.Plum);

            ID.SetControl(ref Decoder_Control);
            ID.SetBrush(Brushes.Green);

            N_Flag.SetControl(ref N_Control);
            N_Flag.SetBrush(Brushes.Orchid);
            Z_Flag.SetControl(ref Z_Control);
            Z_Flag.SetBrush(Brushes.Orchid);
            O_Flag.SetControl(ref O_Control);
            O_Flag.SetBrush(Brushes.Orchid);
            C_Flag.SetControl(ref Carry_Control);
            C_Flag.SetBrush(Brushes.Orchid);

            State.SetControl(ref CurrentIns, ref NextIns);

            //-------------Insert Values to ComboBox-----------
            for (int i = 1; i < 256; i++)
            {
                Input_Register.Items.Add(i.ToString("X2"));
            }

            //-------------Set Variables-----------------------
            //marque = new System.Threading.Thread(Move);
            simulation = new System.Threading.Thread(Simulate);

            Last_Info     = new Info();
            Sim_Info      = new Info[4096];
            Ablauf        = new byte[4096];
            Ablauf_Backup = new byte[4096];
             
            Description_Backup = new ArrayList();

            for (int i = 0; i < 4096; i++)
            {
                Ablauf[i] = Ablauf_Backup[i] = hexa[i];
            }

            foreach (string item in Description)
            {
                RAM.Items.Add(item);
                Description_Backup.Add(item);
            }

            Sim_Info[0] = Set_Info();
        }

        private void Simulate()
        {
            returntofirst = true;
            Perform_F_Fetch();
            Sim_Info[Address_Register.Get()-1] = Set_Info();

            if (Skip) { LoadNextInstruction(); }
            RefreshState();

            returntofirst = false;

            while (PC_Register.Get() != 4096)
            {
                switch (Data_Register.Get())
                {
                    case 16:
                        {//ADD
                            start_address = Address_Register.Get() - 1;
                            Perform_Add();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 17:
                        {//SUB
                            start_address = Address_Register.Get() - 1;
                            Perform_Sub();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 18:
                        {//CMP
                            start_address = Address_Register.Get() - 1;
                            Perform_Cmp();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 19:
                        {//DEC
                            start_address = Address_Register.Get() - 1;
                            Perform_Dec();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 20:
                        {//AND
                            start_address = Address_Register.Get() - 1;
                            Perform_And();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 21:
                        {//OR
                            start_address = Address_Register.Get() - 1;
                            Perform_Or();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 22:
                        {//XOR
                            start_address = Address_Register.Get() - 1;
                            Perform_Xor();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 23:
                        {//CPTA
                            start_address = Address_Register.Get() - 1;
                            Perform_Cpta(); 
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 24:
                        {//RORA
                            start_address = Address_Register.Get() - 1;
                            Perform_Rora();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 25:
                        {//ROLA
                            start_address = Address_Register.Get() - 1;
                            Perform_Rola();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 26:
                        {//RORC
                            start_address = Address_Register.Get() - 1;
                            Perform_Rorc();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 27:
                        {//ROLC
                            start_address = Address_Register.Get() - 1;
                            Perform_Rolc();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 28:
                        {//CLC
                            start_address = Address_Register.Get() - 1;
                            Perform_Clc();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 29:
                        {//CPTC
                            start_address = Address_Register.Get() - 1;
                            Perform_Cptc();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 30:
                        {//INC
                            start_address = Address_Register.Get() - 1;
                            Perform_Inc();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 31:
                        {//HLT
                            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
                            Dispatcher.Invoke(new System.Action(() => clk_enable.Background = Brushes.Red));
                            Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "Halted"));
                            Dispatcher.Invoke(new System.Action(() => Center.Background = (ImageBrush)FindResource("Stop")));
                            simulation.Abort();
                            break;
                        }

                    case 32:
                        {//PMOV A,B
                            start_address = Address_Register.Get() - 1;
                            Perform_Pmov_A_B();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 33:
                        {//PMOV B,A
                            start_address = Address_Register.Get() - 1;
                            Perform_Pmov_B_A();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 34:
                        {//PMOV A,X
                            start_address = Address_Register.Get() - 1;
                            Perform_Pmov_A_X();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 35:
                        {//PMOV X,A 
                            start_address = Address_Register.Get() - 1;
                            Perform_Pmov_X_A();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 36:
                    case 00:
                        {//NOOP
                            start_address = Address_Register.Get() - 1;
                            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 37:
                        {//SWP
                            start_address = Address_Register.Get() - 1;
                            Perform_Swp();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 38:
                        {//INA
                            start_address = Address_Register.Get() - 1;
                            Perform_Ina();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 39:
                        {//OUTA
                            start_address = Address_Register.Get() - 1;
                            Perform_Outa();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 96:
                        {//PMVI A
                            start_address = Address_Register.Get() - 1;
                            Perform_Pmvi_A();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    case 112:
                        {//PMVI X
                            start_address = Address_Register.Get() - 1;
                            Perform_Pmvi_X();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Lda();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Sta();
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Jump(true, "JMP");
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Jump(Z_Flag.Get(), "JZR");
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Jump(!Z_Flag.Get(), "JNZ");
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Jump(C_Flag.Get(), "JCY");
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Jump(!C_Flag.Get(), "JNC");
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Jump(N_Flag.Get(), "JNG");
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Jump(!N_Flag.Get(), "JPS");
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
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
                            start_address = Address_Register.Get() - 1;
                            Perform_Jump(O_Flag.Get(), "JOV");
                            Perform_Fetch();
                            Sim_Info[Address_Register.Get()-1] = Set_Info();
                            
                            if (Skip) { LoadNextInstruction(); }
                            RefreshState();
                            break;
                        }

                    default:
                        {
                            MessageBox.Show("Error in Main Switch , error code : " + Data_Register.Get().ToString("X2"));
                            break;
                        }
                }//end of switch

            }//end of while

        }//end of simulate

        //Simulation Functions

        private void Perform_Add()
        {
            int temp = A_Register.Get() + B_Register.Get();

            O_Flag.Set(false);

            //************************************************************

            if (A_Register.Get() < 128 && B_Register.Get() < 128)     //(+)plus(+)-->(-)
            {
                if (127 < temp && temp < 256)
                {
                    O_Flag.Set(true);
                }
            }
            else if (A_Register.Get() > 127 && B_Register.Get() > 127)//(-)plus(-)-->(+)
            {
                if (255 < temp && temp < 384)
                {
                    O_Flag.Set(true);
                }
            }

            //************************************************************

            if (temp < 256)
            {
                C_Flag.Set(false);
                A_Register.Set((byte)temp);
            }
            else
            {
                C_Flag.Set(true);
                A_Register.Set((byte)(temp - 256));
            }

            //************************************************************

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of ADD"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from registers to ALU & Executing instruction")));

                RefreshAluInput(true, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "ADD"));
                Dispatcher.Invoke(new System.Action(() => s2.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();         //check interval time , increment cycle & animation

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of ADD"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle  

            Dispatcher.Invoke(new System.Action(() => s2.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_B.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of ADD"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get()>127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get()== 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //---------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Sub()
        {
            int B_Content_Complement = 256 - B_Register.Get();
            int temp = A_Register.Get() + B_Content_Complement;
            
            O_Flag.Set(false);

            //************************************************************

            if ( A_Register.Get() < 128  && B_Register.Get() >127 )//(+)minus(-)-->(-)
            {
                if ( 127 < temp && temp < 256 )
                {
                    O_Flag.Set(true);
                }
            }
            else if (A_Register.Get() > 127 && B_Register.Get() <128 )//(-)minus(+)-->(+)
            {
                if ( 255 < temp && temp < 384 )
                {
                    O_Flag.Set(true);
                }
            }

            //************************************************************

            if (temp < 256)
            {
                C_Flag.Set(true);
                A_Register.Set((byte)temp);
            }
            else
            {
                C_Flag.Set(false);
                A_Register.Set((byte)(temp - 256));
            }

            //************************************************************

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of SUB"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from registers to ALU & Executing instruction")));

                RefreshAluInput(true, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "SUB"));
                Dispatcher.Invoke(new System.Action(() => s0.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s1.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s2.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s11.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of SUB"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => s0.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s1.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s2.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s11.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_B.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of SUB"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Cmp()
        {
            int A_Content_Temp = A_Register.Get();
            int B_Content_Complement = 256 - B_Register.Get();
            int temp = A_Register.Get() + B_Content_Complement;

            O_Flag.Set(false);

            //************************************************************

            if (A_Register.Get() < 128 && B_Register.Get() > 127)//(+)minus(-)-->(-)
            {
                if (127 < temp && temp < 256)
                {
                    O_Flag.Set(true);
                }
            }
            else if (A_Register.Get() > 127 && B_Register.Get() < 128)//(-)minus(+)-->(+)
            {
                if (255 < temp && temp < 384)
                {
                    O_Flag.Set(true);
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

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of CMP"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from registers to ALU & Executing instruction")));

                RefreshAluInput(true, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "CMP"));
                Dispatcher.Invoke(new System.Action(() => s0.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s1.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s2.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of CMP"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing Carry & Overflow flag")));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => s0.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s1.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s2.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_B.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of CMP"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Content_Temp>127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Content_Temp==0)));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));

            return;
        }

        private void Perform_Dec()
        {
            if (A_Register.Get() == 128)//A is smallest negative number so decreasing it causes overflow
            {
                O_Flag.Set(true);//sum of two negative number causes overflow
            }
            else
            {
                O_Flag.Set(false);
            }

            if (A_Register.Get() == 0)
            {
                A_Register.Set(255);
            }
            else
            {
                A_Register.Set((byte)(A_Register.Get() - 1));
            }

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of DEC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from registers to ALU & Executing instruction")));

                RefreshAluInput(false, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "DEC"));
                Dispatcher.Invoke(new System.Action(() => s0.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s1.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of DEC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //--------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => s0.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s1.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of DEC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_And()
        {
            A_Register.Set((byte)(A_Register.Get() & B_Register.Get()));
            O_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "fist cycle of AND"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from registers to ALU & Executing instruction")));

                RefreshAluInput(true, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "AND"));
                Dispatcher.Invoke(new System.Action(() => s6.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of AND"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle  

            Dispatcher.Invoke(new System.Action(() => s6.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_B.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of ADD"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //---------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Or()
        {
            A_Register.Set((byte)(A_Register.Get() | B_Register.Get()));
            O_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "fist cycle of OR"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from registers to ALU & Executing instruction")));

                RefreshAluInput(true, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "OR"));
                Dispatcher.Invoke(new System.Action(() => s7.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of OR"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle  

            Dispatcher.Invoke(new System.Action(() => s7.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_B.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of OR"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //---------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Xor()
        {
            A_Register.Set((byte)(A_Register.Get() ^ B_Register.Get()));
            O_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "fist cycle of XOR"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from registers to ALU & Executing instruction")));

                RefreshAluInput(true, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "XOR"));
                Dispatcher.Invoke(new System.Action(() => s3.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s6.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s7.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of XOR"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle  

            Dispatcher.Invoke(new System.Action(() => s3.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s6.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s7.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_B.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of XOR"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //---------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Cpta()
        {
            A_Register.Set((byte)(255 - A_Register.Get()));//check whether 2's complement or 1's complement
            O_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of CPTA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to ALU & Executing instruction")));

                RefreshAluInput(false, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "CPTA"));
                Dispatcher.Invoke(new System.Action(() => s0.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s1.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s2.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of CPTA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Overflow flag")));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //--------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => s0.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s1.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s2.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of CPTA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Rora()
        {
            byte A_Content_Temp = (byte)(A_Register.Get() / 2);

            if ((A_Register.Get() % 2) == 1)
            {
                C_Flag.Set(true);
                A_Content_Temp += 128;
            }
            else
            {
                C_Flag.Set(false);
            }

            A_Register.Set(A_Content_Temp);
            O_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of RORA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to ALU & Executing instruction")));

                RefreshAluInput(false, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "RORA"));
                Dispatcher.Invoke(new System.Action(() => s10.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of RORA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => s10.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of RORA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Negative flag")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Rola()
        {
            int A_Content_Temp = A_Register.Get() * 2;

            if (A_Content_Temp > 255)
            {
                C_Flag.Set(true);
                A_Content_Temp ++;
                A_Register.Set((byte)(A_Content_Temp - 256));
            }
            else
            {
                C_Flag.Set(false);
                A_Register.Set((byte)A_Content_Temp);
            }

            O_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of ROLA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to ALU & Executing instruction")));

                RefreshAluInput(false, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "ROLA"));
                Dispatcher.Invoke(new System.Action(() => s4.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s10.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of ROLA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => s4.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s10.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of ROLA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Negative flag")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Rorc()
        {
            int A_Content_Temp;

            if (C_Flag.Get())
            {
                A_Content_Temp = A_Register.Get() + 256;
            }
            else
            {
                A_Content_Temp = A_Register.Get();
            }

            if ((A_Register.Get() % 2) == 1)
            {
                C_Flag.Set(true);
            }
            else
            {
                C_Flag.Set(false);
            }

            A_Register.Set((byte)(A_Content_Temp / 2));
            O_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of RORC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to ALU & Executing instruction")));

                RefreshAluInput(false, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "RORC"));
                Dispatcher.Invoke(new System.Action(() => s5.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s10.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of RORC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, (timing / 2));
            }

            Check();

            //----------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => s5.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s10.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of RORC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Rolc()
        {
            int A_Content_Temp = A_Register.Get() * 2;

            if (C_Flag.Get())
            {
                A_Content_Temp++;
            }

            if (A_Content_Temp > 255)
            {
                C_Flag.Set(true);
                A_Register.Set((byte)(A_Content_Temp - 256));
            }
            else
            {
                C_Flag.Set(false);
                A_Register.Set((byte)A_Content_Temp);
            }

            O_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of ROLC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to ALU & Executing instruction")));

                RefreshAluInput(false, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "ROLC"));
                Dispatcher.Invoke(new System.Action(() => s4.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s5.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s10.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of ROLC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Carry and Overflow flags")));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, timing);
            }

            Check();

            //----------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => s4.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s5.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s10.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));

            Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of ROLC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Clc()
        {
            C_Flag.Set(false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "CLC"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of CLC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Executing instruction")));
                Dispatcher.Invoke(new System.Action(() => s9.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "CLC"));

                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of CLC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing Carry flag")));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => s9.Background = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Hide()));

            if (animation_on) { Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE")); }

            return;
        }

        private void Perform_Cptc()
        {
            C_Flag.Set(!C_Flag.Get());

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //----------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "CPTC"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of CPTC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Executing instruction")));
                Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Blue));
                Dispatcher.Invoke(new System.Action(() => s9.Background = Brushes.Blue));

                System.Threading.Thread.Sleep(timing);
            }

            Check();

            //----------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "CPTC"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of CPTC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing Carry flag")));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => s8.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => s9.Background = Brushes.Red));
            Dispatcher.Invoke(new System.Action(() => C_Flag.Clock_Hide()));

            if (animation_on) { Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE")); }

            return;
        }

        private void Perform_Inc()
        {
            if (A_Register.Get() == 127)//A is biggest positive number so increasing it causes overflow
            {
                O_Flag.Set(true);//sum of two positive number causes overflow
            }
            else
            {
                O_Flag.Set(false);
            }

            if (A_Register.Get() == 255)
            {
                A_Register.Set(0);
            }
            else
            {
                A_Register.Set((byte)(A_Register.Get() + 1));
            }

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of INC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to ALU & Executing instruction")));

                RefreshAluInput(false, (timing / 2));

                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "INC"));

                System.Threading.Thread.Sleep(timing / 2);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of INC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving result from ALU to data bus & Refreshing Overflow flag")));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh()));

                Perform_Transfer(2, timing);
            }

            Check();

            //--------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => O_Flag.Clock_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => ALU_Status.Text = "IDLE"));
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of INC"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing result in A & Refreshing Zero and Negative flags")));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value = 0));

            return;
        }

        private void Perform_Pmov_A_B()
        {
            A_Register.Set(B_Register.Get());
            N_Flag.Set(A_Register.Get() > 128 ? true : false);
            Z_Flag.Set(A_Register.Get() == 0 ? true : false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of PMOV A,B"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from B to data bus")));

                Perform_Transfer(1, timing);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of PMOV A,B"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from data bus to A")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => B_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));

            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => B_Output_2.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));

            return;
        }

        private void Perform_Pmov_B_A()
        {
            B_Register.Set(A_Register.Get());

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of PMOV B,A"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to data bus")));

                Perform_Transfer(8, timing);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of PMOV B,A"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from data bus to B")));

                Dispatcher.Invoke(new System.Action(() => B_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => B_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => A_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));

            Dispatcher.Invoke(new System.Action(() => B_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Input_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Input_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => B_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => B_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Output_2.Visibility = Visibility.Hidden));

            return;
        }

        private void Perform_Pmov_A_X()
        {
            A_Register.Set(X_Register.Get());
            N_Flag.Set(A_Register.Get() > 128 ? true : false);
            Z_Flag.Set(A_Register.Get() == 0 ? true : false);

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of PMOV A,X"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from X to data bus")));

                Perform_Transfer(3, timing);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of PMOV A,X"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from data bus to A")));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(A_Register.Get() > 127)));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(A_Register.Get() == 0)));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => X_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => X_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => X_Output_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => X_Output_2.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));

            return;
        }

        private void Perform_Pmov_X_A()
        {
            X_Register.Set(A_Register.Get());

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of PMOV X,A"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to data bus")));

                Perform_Transfer(9, timing);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of PMOV X,A"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from data bus to X")));

                Dispatcher.Invoke(new System.Action(() => X_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => X_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => A_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => X_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => X_Input_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => X_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Output_2.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => X_Register.Clock_Hide()));

            return;
        }

        private void Perform_Swp()
        {
            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            byte A_Content_Temp = A_Register.Get();

            A_Register.Set(B_Register.Get());
            B_Register.Set(A_Content_Temp);

            N_Flag.Set(A_Register.Get() > 127 ? true : false);
            Z_Flag.Set(A_Register.Get() == 0 ? true : false);

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of SWP"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from B to data bus & from A to input of B")));

                Dispatcher.Invoke(new System.Action(() => MUX_B_0.Fill = Brushes.Red));
                Dispatcher.Invoke(new System.Action(() => MUX_B_1.Fill = Brushes.LightGreen));

                Perform_Transfer(12, timing);  //Swap
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of SWP"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from data bus to A & from output of A to B")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));

                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => B_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh()));

                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => B_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch 

            Dispatcher.Invoke(new System.Action(() => B_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => MUX_B_0.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => MUX_B_1.Fill = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => MUX_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => MUX_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => MUX_3.Value = 0));
            Dispatcher.Invoke(new System.Action(() => MUX_4.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));

            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Input_3.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => B_Output_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => B_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => B_Output_2.Visibility = Visibility.Hidden));

            return;

        }//Check again

        private void Perform_Ina()
        {
            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            Dispatcher.Invoke(new System.Action(() => In_Content = Convert.ToByte(Input_Register.SelectedIndex)));
            A_Register.Set(In_Content);

            N_Flag.Set(A_Register.Get() > 128 ? true : false);
            Z_Flag.Set(A_Register.Get() == 0 ? true : false);

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of INA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from input to data bus")));

                Dispatcher.Invoke(new System.Action(() => Input_Register.IsEnabled = false));

                Perform_Transfer(4, timing);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of INA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from data bus to A")));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => Input_Register.IsEnabled = true));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => Input_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Out_Port_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Out_Port_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            //Dispatcher.Invoke(new System.Action(() => In_Port_2.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));

            return;
        }

        private void Perform_Outa()
        {
            Output_Register.Set(A_Register.Get());

            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of OUTA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to data bus")));

                Perform_Transfer(10, timing);
            }

            Check();

            //--------------------------------------------------------second cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of OUTA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from data bus to output")));

                Dispatcher.Invoke(new System.Action(() => Output_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => Output_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => A_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Output_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Out_In.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => In_Port_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => In_Port_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => A_Output_2.Visibility = Visibility.Hidden));

            return;
        }

        private void Perform_Lda()
        {
            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            HTemp_Register.Set((byte)(Data_Register.Get() % 16));
            Data_Register.Set(Ablauf[Address_Register.Get()]);

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of LDA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing low nibble of opcode in HTemp & Fetching second byte")));

                RefreshRegisterInput((timing / 3), 10);                  //Read & HTemp & RW_clock

                Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh()));

                Perform_Transfer(7, (timing / 3));                       //Move Data to LTemp
            }

            Check();

            //--------------------------------------------------------second cycle

            LTemp_Register.Set(Data_Register.Get());
            Address_Register.Set((256 * HTemp_Register.Get()) + (LTemp_Register.Get()));

            Dispatcher.Invoke(new System.Action(() => HTemp_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => RW_clock.Value   = 0));
            Dispatcher.Invoke(new System.Action(() => HTemp_In_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => HTemp_In_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of LDA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving 2nd byte from memory to LTEMP & Locating generated address on Address Bus")));

                //Dispatcher.Invoke(new System.Action(() => MUX_Address_0.Fill = Brushes.Red));
                //Dispatcher.Invoke(new System.Action(() => MUX_Address_1.Fill = Brushes.LightGreen));

                Dispatcher.Invoke(new System.Action(() => LTemp_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 3));

                Dispatcher.Invoke(new System.Action(() => LTemp_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => LTemp_Input.Value = 0));

                RefreshRegisterOutput((timing / 3), 8 );                      //First  part of Address_Content to Address_Bus(LDA address modification)
                RefreshRegisterOutput((timing / 3), 12);                      //Second part of  "       "
            }

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
            Dispatcher.Invoke(new System.Action(() => LTemp_Input.Value = 0));

            Check();

            //--------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => RD_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => LTemp_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of LDA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory")));

                RefreshRegisterInput((timing / 2), 7);                //RW_clock ??????

                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Temp_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => HTemp_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => LTemp_Output.Value = 0));

                System.Threading.Thread.Sleep((timing / 2));          //Misspend time
            }

            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Temp_Output.Value = 0));
            Dispatcher.Invoke(new System.Action(() => HTemp_Output.Value = 0));
            Dispatcher.Invoke(new System.Action(() => LTemp_Output.Value = 0));

            Check();

            //--------------------------------------------------------fourth cycle

            PC_Register.Increment();
            Data_Register.Set(Ablauf[Address_Register.Get()]);     //Data to Load

            Address_Register.Set(PC_Register.Get());

            Dispatcher.Invoke(new System.Action(() => MUX_Address_0.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => MUX_Address_1.Fill = Brushes.Red));

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "4th cycle of LDA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Incrementing Program Counter")));

                Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh()));

                //RefreshRegisterOutput((timing / 2), 9);                 //Read 
                RefreshRegisterInput((timing / 2), 6);               //Read & Address Bus
            }

            Check();

            //--------------------------------------------------------fifth cycle

            Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "5th cycle of LDA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing Memory & Moving data from memory to data bus")));

                RefreshRegisterInput((timing / 3), 7);               //RW_clock

                Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                Perform_Transfer(5, (timing / 3));                   //Move Data to A
            }

            Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

            Check();

            //--------------------------------------------------------sixth cycle

            A_Register.Set(Data_Register.Get());

            N_Flag.Set(A_Register.Get() > 127 ? true : false);
            Z_Flag.Set(A_Register.Get() == 0 ? true : false);

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "6th cycle of LDA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from data bus to A")));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
                Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => RD_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));

            return;
        }

        private void Perform_Sta()
        {
            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            HTemp_Register.Set((byte)(Data_Register.Get() % 16));
            Data_Register.Set(Ablauf[Address_Register.Get()]);

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() =>CU_Status.Text = "1st cycle of STA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Storing low nibble of opcode in HTemp & Fetching second byte")));

                RefreshRegisterInput((timing / 3), 10);              //clk_HTemp & clk_RW & Read

                Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh())); ;

                Perform_Transfer(7, (timing / 3));                   //Move Data to LTemp
            }

            Check();

            //--------------------------------------------------------second cycle

            LTemp_Register.Set(Data_Register.Get());
            Address_Register.Set((256 * HTemp_Register.Get()) + (LTemp_Register.Get()));

            Dispatcher.Invoke(new System.Action(() => HTemp_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => HTemp_In_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => HTemp_In_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of STA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving 2nd byte from memory to LTEMP & Locating generated address on Address Bus")));

                Dispatcher.Invoke(new System.Action(() => LTemp_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => LTemp_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => LTemp_Input.Value = 0));

                RefreshRegisterOutput((timing / 2), 8);                      //Move to Address_Content to Address Bus(STA address modification)
            }

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
            Dispatcher.Invoke(new System.Action(() => LTemp_Input.Value = 0));

            Check();

            //--------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => RD_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => LTemp_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of STA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving data from A to data bus")));

                RefreshRegisterOutput(timing / 2, 12);//second part of address modification
                Perform_Transfer(11, timing/2 );                          //Move A to Memory & RW_enable
            }

            Check();

            //--------------------------------------------------------fourth cycle

            PC_Register.Increment();
            Q_Register.Set(A_Register.Get());

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "4th cycle of STA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Incrementing Program Counter & Refreshing memory")));

                Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Show()));

                System.Threading.Thread.Sleep(timing / 3);

                Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh()));

                RefreshRegisterInput((timing / 3), 7);                //RW_clock 

                Dispatcher.Invoke(new System.Action(() => Q_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => WR_Register.Text = "1"));

                Dispatcher.Invoke(new System.Action(() => RAM.SelectedIndex = Address_Register.Get()));

                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Temp_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => HTemp_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => LTemp_Output.Value = 0));

                Thread.Sleep((timing / 3));
            }
            else
            {
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Temp_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => HTemp_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => LTemp_Output.Value = 0));
            }

            Check();

            //--------------------------------------------------------fifth cycle

            Ablauf[Address_Register.Get()] = Q_Register.Get();        //rethink about this assigment

            ModifyRamContent(Address_Register.Get());

            Address_Register.Set(PC_Register.Get());

            Dispatcher.Invoke(new System.Action(() => WR.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));

            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Output_2.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => A_Output_2.Visibility = Visibility.Hidden));
            Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_1.Visibility = Visibility.Hidden));
            //Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_2.Visibility = Visibility.Hidden));
            //Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_3.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Hide()));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => A_Buffer.Fill = Brushes.Pink));
            Dispatcher.Invoke(new System.Action(() => MUX_Address_0.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => MUX_Address_1.Fill = Brushes.Red));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "5th cycle of STA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving address from PC to address bus")));

                RefreshAddressBusContent(timing);
            }

            Check();

            //--------------------------------------------------------sixth cycle

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "6th cycle of STA"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory")));

                RefreshRegisterInput((timing / 2), 7);                          //RW_clock 

                Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => WR_Register.Text = "0"));

                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                Thread.Sleep((timing / 2));
            }

            Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            return;
        }

        private void Perform_Pmvi_A()
        {
            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            Data_Register.Set(Ablauf[Address_Register.Get()]);

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of PMVI A"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving second byte from memory to data bus")));

                RefreshRegisterInput((timing / 2), 8);                  //Read & RW_clock

                Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => RAM.SelectedIndex = Address_Register.Get()));

                Perform_Transfer(5, (timing / 2));                      //Move Data to A
            }

            Check();

            //--------------------------------------------------------second cycle

            PC_Register.Increment();

            Address_Register.Set(PC_Register.Get());

            A_Register.Set(Data_Register.Get());
            N_Flag.Set(A_Register.Get() > 127 ? true : false);
            Z_Flag.Set(A_Register.Get() == 0 ? true : false);

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of PMVI A"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Incrementing Program Counter & Moving data from data bus to A & Refreshing Zero & Negative flags")));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Show()));

                Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh()));
                Dispatcher.Invoke(new System.Action(() => A_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh()));

                RefreshAddressBusContent((timing / 2));                         //Move Address_Content to Address_Bus
            }

            Check();

            //--------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => RD_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));

            Dispatcher.Invoke(new System.Action(() => Z_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => A_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of PMVI A"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory")));

                RefreshRegisterInput((timing / 2), 7);                //RW_clock

                Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                Thread.Sleep((timing / 2));          //Misspend time
            }
            else
            {
                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));
            }

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            return;
        }

        private void Perform_Pmvi_X()
        {
            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //--------------------------------------------------------first cycle

            Data_Register.Set(Ablauf[Address_Register.Get()]);

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of PMVI X"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving second byte from memory to data bus")));

                RefreshRegisterInput((timing / 2), 8);                    //Read & RW_clock

                Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => RAM.SelectedIndex = Address_Register.Get()));

                Perform_Transfer(13, (timing / 2));                       //Move Data to X
            }

            Check();

            //--------------------------------------------------------second cycle

            Address_Register.Set(PC_Register.Increment());

            X_Register.Set(Data_Register.Get());

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            Dispatcher.Invoke(new System.Action(() => Deactivate()));
            Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
            Dispatcher.Invoke(new System.Action(() => X_Input_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => X_Input_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => X_Input_1.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of PMVI X"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Incrementing Program Counter & Moving data from data bus to X")));

                Dispatcher.Invoke(new System.Action(() => X_Register.Clock_Show()));
                Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => X_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh()));

                RefreshAddressBusContent((timing / 2));                         //Move Address_Content to Address_Bus
            }

            Check();

            //--------------------------------------------------------third cycle

            Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Hide()));
            Dispatcher.Invoke(new System.Action(() => X_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => RD_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of PMVI X"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory")));

                RefreshRegisterInput((timing / 2), 7);                  //RW_clock

                Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                System.Threading.Thread.Sleep((timing / 2));          //Misspend time
            }

            Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

            Check();

            //--------------------------------------------------------fetch

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            return;
        }

        private void Perform_Jump(bool type, string text)
        {
            if (type)//Successful Jump
            {
                Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
                Tcycle = 0;
                if(!move_requested){Tselect=0;}
                Check();

                //--------------------------------------------------------first cycle 

                HTemp_Register.Set((byte)(Data_Register.Get() % 16));

                Data_Register.Set(Ablauf[Address_Register.Get()]);

                if (animation_on)
                {
                    Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of " + text + " (+)"));
                    Dispatcher.Invoke(new System.Action(() => State.Text = "Storing low nibble of opcode in HTemp & Fetching second byte"));

                    RefreshRegisterInput((timing / 2), 10);                   //Read & RW_clock & clk_HTemp

                    Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh()));

                    Perform_Transfer(7, (timing / 2));                        //Move Data to LTemp
                }

                Check();

                //--------------------------------------------------------second cycle

                LTemp_Register.Set(Data_Register.Get());
                Address_Register.Set((64 * HTemp_Register.Get()) + (LTemp_Register.Get()));

                Dispatcher.Invoke(new System.Action(() => HTemp_Register.Clock_Hide()));

                Dispatcher.Invoke(new System.Action(() => RW_clock.Value   = 0));
                Dispatcher.Invoke(new System.Action(() => HTemp_In_1.Value = 0));
                Dispatcher.Invoke(new System.Action(() => HTemp_In_2.Value = 0));

                Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

                if (animation_on)
                {
                    Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of " + text + " (+)"));
                    Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving 2nd byte from data bus to LTemp & generated address from Temp to PC")));

                    Dispatcher.Invoke(new System.Action(() => LTemp_Register.Clock_Show()));

                    System.Threading.Thread.Sleep((timing / 2));

                    Dispatcher.Invoke(new System.Action(() => LTemp_Register.Refresh()));
                    Dispatcher.Invoke(new System.Action(() => Deactivate()));
                    Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => LTemp_Input.Value = 0));

                    RefreshRegisterOutput((timing / 2), 11);
                }
                else
                {
                    Dispatcher.Invoke(new System.Action(() => Deactivate()));
                    Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => LTemp_Input.Value = 0));
                }

                Check();

                //--------------------------------------------------------third cycle

                PC_Register.Set(Address_Register.Get());

                Dispatcher.Invoke(new System.Action(() => RD_1.Value = 0));
                Dispatcher.Invoke(new System.Action(() => RD_2.Value = 0));
                Dispatcher.Invoke(new System.Action(() => RD_3.Value = 0));

                Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.LightGreen));
                Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));

                if (animation_on)
                {
                    Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of " + text + " (+)"));

                    Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing Program Counter & Moving PC to address bus")));

                    Dispatcher.Invoke(new System.Action(() => PC_Register.Clock_Show()));

                    System.Threading.Thread.Sleep((timing / 2));

                    Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh()));

                    Dispatcher.Invoke(new System.Action(() => PC_In.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => Temp_Output.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => LTemp_Output.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => HTemp_Output.Value = 0));

                    RefreshAddressBusContent((timing / 2));
                    Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Hidden));
                }
                else
                {
                    Dispatcher.Invoke(new System.Action(() => PC_In.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => Temp_Output.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => LTemp_Output.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => HTemp_Output.Value = 0));
                }

                Check();

                //--------------------------------------------------------fourth cycle

                Dispatcher.Invoke(new System.Action(() => PC_Register.Clock_Hide()));

                if (animation_on)
                {
                    Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "4th cycle of " + text + " (+)"));
                    Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory")));

                    RefreshRegisterInput((timing / 2), 7);                    //RW_clock

                    Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                    Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                    System.Threading.Thread.Sleep((timing / 2));
                }

                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                Check();

                //--------------------------------------------------------fetch

                Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            }
            else    //Unsuccessful Jump
            {
                Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value = 0));
                Tcycle = 0;
                if(!move_requested){Tselect=0;}
                Check();

                //--------------------------------------------------------first cycle 

                PC_Register.Increment();
                Address_Register.Set(PC_Register.Get());

                if (animation_on)
                {
                    Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of " + text + " (-)"));
                    Dispatcher.Invoke(new System.Action(() => State.SetMessage("Incrementing Program Counter & Moving PC to address bus")));

                    Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Show()));

                    System.Threading.Thread.Sleep((timing / 2));

                    Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh()));

                    RefreshAddressBusContent((timing / 2));
                }

                Check();

                //--------------------------------------------------------second cycle 

                if (animation_on)
                {
                    Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of " + text + " (-)"));
                    Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory")));

                    RefreshRegisterInput((timing / 2), 7);                    //RW_clock

                    Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                    Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                    Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                    System.Threading.Thread.Sleep((timing / 2));
                }

                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                Check();

                //--------------------------------------------------------fetch

                Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
                Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));
            }

            return;
        }

        private void Perform_F_Fetch()
        {
            Tcycle = 0;
            if(!move_requested){Tselect=0;}
            Check();

            //-------------------------------------------1.fetch

            Address_Register.Set(PC_Register.Get());           //000H
            Data_Register.Set(Ablauf[Address_Register.Get()]);

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of f_fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory")));

                RefreshRegisterInput((timing / 2), 9);        //Clock_RW & Address

                Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //-------------------------------------------2.fetch

            Address_Register.Set(PC_Register.Increment());     //001H

            Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            //Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of f_fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Incrementing Program Counter")));

                Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh()));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //------------------------------------------3.fetch

            IR_Register.Set(Data_Register.Get());

            Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of f_fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory & Moving opcode from memory to data bus")));

                RefreshRegisterInput((timing / 2), 11); //Attention!!!

                Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                Dispatcher.Invoke(new System.Action(() => RAM.SelectedIndex = Address_Register.Get() - 1));

                Perform_Transfer(6, (timing / 2));      //Move Data to IR
            }
            else
            {
                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));
            }

            Check();

            //------------------------------------------4.fetch

            IR_Register.Set(Data_Register.Get());

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "4th cycle of f_fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving opcode from data bus to IR")));

                Dispatcher.Invoke(new System.Action(() => IR_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => IR_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Deactivate()));
                Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => IR_Input.Value = 0));

                System.Threading.Thread.Sleep((timing / 2));
            }
            else
            {
                Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => IR_Input.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Deactivate()));
            }

            Check();

            //-----------------------------------------5.fetch

            Dispatcher.Invoke(new System.Action(() => IR_Register.Clock_Hide()));

            Dispatcher.Invoke(new System.Action(() => RD_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "5th cycle of f_fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving opcode from IR to ID & Mapping opcode to corresponding address")));

                RefreshRegisterOutput((timing / 2), 6);

                Dispatcher.Invoke(new System.Action(() => ID.Set(Data_Register.Get(), N_Flag.Get(), Z_Flag.Get(), O_Flag.Get(), C_Flag.Get())));

                System.Threading.Thread.Sleep((timing / 2));
            }

            Check();

            //-----------------------------------------6.fetch

            Dispatcher.Invoke(new System.Action(() => ID.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => IR_Output.Value = 0));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "6th cycle of f_fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving mapped address to Control Unit")));

                RefreshRegisterOutput(timing, 7);
            }

            //-----------------------------------------

            if(!move_requested){Tselect=0;}
            return;
        }

        private void Perform_Fetch()
        {
            Check();

            //-------------------------------------------1.fetch

            Data_Register.Set(Ablauf[Address_Register.Get()]);
            Address_Register.Set(PC_Register.Increment());

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "1st cycle of fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Incrementing Program Counter")));
                Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh()));

                RefreshRegisterInput((timing / 2), 6);
            }

            Check();

            //-------------------------------------------2.fetch

            Dispatcher.Invoke(new System.Action(() => PC_Register.Inc_Hide()));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "2nd cycle of fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Refreshing memory & Moving opcode from memory to data bus")));

                RefreshRegisterInput((timing / 2), 7);

                Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
                Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh()));

                Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

                Dispatcher.Invoke(new System.Action(() => RAM.SelectedIndex = Address_Register.Get() - 1));

                Perform_Transfer(6, (timing / 2));
            }

            Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value = 0));

            Check();

            //------------------------------------------3.fetch

            IR_Register.Set(Data_Register.Get());

            Dispatcher.Invoke(new System.Action(() => RW_clock.Value = 0));
            Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Hidden));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "3rd cycle of fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving opcode from data bus to IR")));
                Dispatcher.Invoke(new System.Action(() => IR_Register.Clock_Show()));

                System.Threading.Thread.Sleep((timing / 2));

                Dispatcher.Invoke(new System.Action(() => IR_Register.Refresh()));
                Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => IR_Input.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Deactivate()));

                System.Threading.Thread.Sleep((timing / 2));
            }
            else
            {
                Dispatcher.Invoke(new System.Action(() => Data_Bus.Value = 0));
                Dispatcher.Invoke(new System.Action(() => IR_Input.Value = 0));
                Dispatcher.Invoke(new System.Action(() => Deactivate()));
            }

            Check();

            //------------------------------------------4.fetch

            Dispatcher.Invoke(new System.Action(() => IR_Register.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => RD_1.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_2.Value = 0));
            Dispatcher.Invoke(new System.Action(() => RD_3.Value = 0));

            Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.LightGreen));
            Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "4th cycle of fetch"));

                RefreshRegisterOutput((timing / 2), 6);     //IR_Output

                Dispatcher.Invoke(new System.Action(() => ID.Set(Data_Register.Get(), N_Flag.Get(), Z_Flag.Get(), O_Flag.Get(), C_Flag.Get())));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving opcode from IR to ID & Mapping opcode to corresponding address")));

                System.Threading.Thread.Sleep((timing / 2));  //Misspend Time
            }

            Check();

            //-----------------------------------------5.fetch

            Dispatcher.Invoke(new System.Action(() => ID.Clock_Hide()));
            Dispatcher.Invoke(new System.Action(() => IR_Output.Value = 0));

            if (animation_on)
            {
                Dispatcher.Invoke(new System.Action(() => CU_Status.Text = "5th cycle of fetch"));
                Dispatcher.Invoke(new System.Action(() => State.SetMessage("Moving mapped address to Control Unit")));

                RefreshRegisterOutput(timing, 7);
            }

            //-----------------------------------------
            if(!move_requested){Tselect=0;}
            return;
        }

        //------------------------------------//--------------------------------------------------//

        private int  GetSelectedItem()
        {
            switch ((string)Timing.SelectionBoxItem)
            {
                case "2000 ms":
                    {
                        return 2000;
                    }

                case "3000 ms":
                    {
                        return 3000;
                    }

                case "4000 ms":
                    {
                        return 4000;
                    }

                case "5000 ms":
                    {
                        return 5000;
                    }

                case "6000 ms":
                    {
                        return 6000;
                    }

                case "7000 ms":
                    {
                        return 7000;
                    }

                case "8000 ms":
                    {
                        return 8000;
                    }

                case "9000 ms":
                    {
                        return 9000;
                    }

                case "10000 ms":
                    {
                        return 10000;
                    }

                default:
                    {
                        return 5000;
                    }
            }

        }

        private void Check()
        {
            Dispatcher.Invoke(new System.Action(() => timing = GetSelectedItem()));

            if (!reset)
            {
                if (Tcycle != Tselect)
                {
                    if (Tcycle > Tselect)    //Previous
                    {
                        animation_on = false;
                        Skip = true;
                    }
                    else                     //Next
                    {
                        animation_on = false;
                    }
                }
                else
                {
                    animation_on = true;
                    move_requested = false;
                    Tselect++;
                }

                Tcycle++;
            }
            else
            {
                Dispatcher.Invoke(new System.Action(() => N_Flag.Reset_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Reset_Show()));
                Dispatcher.Invoke(new System.Action(() => PC_Register.Reset_Show()));
                System.Threading.Thread.Sleep(timing);

                Reset();
            }
        }

        private void ModifyRamContent(int address)
        {
            string new_string = "";

            if ((Ablauf[address - 1] / 16) < 4)//One Byte Instruction
            {         
                switch (Ablauf[address])
                {
                    case 00:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") ;
                        break;

                    case 16:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "ADD";
                        break;

                    case 17:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "SUB";
                        break;

                    case 18:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "CMP";
                        break;

                    case 19:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "DEC";
                        break;

                    case 20:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "AND";
                        break;

                    case 21:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "OR";
                        break;

                    case 22:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "XOR";
                        break;

                    case 23:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "CPTA";
                        break;

                    case 24:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "RORA";
                        break;

                    case 25:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "ROLA";
                        break;

                    case 26:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "RORC";
                        break;

                    case 27:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "ROLC";
                        break;

                    case 28:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "CLC";
                        break;

                    case 29:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "CPTC";
                        break;

                    case 30:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "INC";
                        break;

                    case 31:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "HLT";
                        break;

                    case 32:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "PMOV A,B";
                        break;

                    case 33:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "PMOV B,A";
                        break;

                    case 34:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "PMOV A,X";
                        break;

                    case 35:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "PMOV X,A";
                        break;

                    case 36:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "NOOP";
                        break;

                    case 37:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "SWP";
                        break;

                    case 38:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "INA";
                        break;

                    case 39:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "OUTA";
                        break;

                    case 96:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "PMVI A,";
                        Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
                        break;

                    case 112:
                        new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "PMVI X,";
                        Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
                        break;

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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "LDA";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "STA";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "JMP";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address+1).ToString("X3") + " : " + Ablauf[address+1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "JZR";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "JNZ";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "JCY";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "JNC";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "JNG";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "JPS";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
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
                            new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "JOV";
                            Dispatcher.Invoke(new System.Action(() => RAM.Items[address + 1] = (address + 1).ToString("X3") + " : " + Ablauf[address + 1].ToString("X2") + " : " + "2nd Byte"));
                            break;
                        }
                }

            }
            else//Two Byte Instruction
            {
                new_string = address.ToString("X3") + " : " + Ablauf[address].ToString("X2") + " : " + "2nd Byte";
            }

            Dispatcher.Invoke(new System.Action(() => RAM.Items[address] = new_string));

            return;
        }//2 Byte Instructions

        private void Perform_Transfer(int type, int local_timing)
        {
            switch (type)
            {
                case 1://Move B to A
                    {
                        local_timing /= 3;

                        RefreshRegisterOutput(local_timing, 2);

                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));

                        RefreshOutIn(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 2://Move ALU to A
                    {
                        local_timing /= 3;

                        RefreshRegisterOutput(local_timing, 3);

                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));

                        RefreshOutIn(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 3://Move X to A
                    {
                        local_timing /= 3;

                        RefreshRegisterOutput(local_timing, 4);

                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));

                        RefreshOutIn(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 4://Move Inport to A
                    {
                        local_timing /= 3;

                        RefreshRegisterOutput(local_timing, 5);

                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));

                        RefreshOutIn(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 5://Move Data to A
                    {
                        local_timing /= 2;

                        RefreshDataBusContent(local_timing, 3);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 6://Move Data to IR
                    {
                        local_timing /= 2;
                        
                        RefreshDataBusContent(local_timing, 2);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        Dispatcher.Invoke(new System.Action(() => IR_Input.Value = 100));

                        System.Threading.Thread.Sleep(local_timing);

                        return;
                    }

                case 7://Move Data to Low Temp
                    {
                        local_timing /= 2;

                        RefreshDataBusContent(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        Dispatcher.Invoke(new System.Action(() => LTemp_Input.Value = 100));

                        System.Threading.Thread.Sleep(local_timing);

                        return;
                    }

                case 8://Move A to B
                    {
                        local_timing /= 3;

                        RefreshRegisterOutput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));

                        RefreshOutIn(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 2);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }


                case 9://Move A to X
                    {
                        local_timing /= 3;

                        RefreshRegisterOutput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));

                        RefreshOutIn(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 4);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }


                case 10://Move A to Outport
                    {
                        local_timing /= 3;

                        RefreshRegisterOutput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));

                        RefreshOutIn(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 5);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }


                case 11://Move A to Memory
                    {
                        local_timing /= 2;

                        RefreshRegisterOutput(local_timing, 1);

                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));

                        RefreshDataBusContent(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 12://Swap
                    {
                        local_timing /= 40;

                        int tempo = 0;

                        while (tempo != 100)//10
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() =>B_Output_1.Value += 10));
                            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value += 2));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => B_Buffer.Fill = Brushes.LightGreen));
                        Dispatcher.Invoke(new System.Action(() => B_Output_2.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Text = A_Register.Get().ToString("X2")));//instead of B
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));

                        tempo = 0;

                        while (tempo != 100)//4
                        {
                            tempo += 25;
                            Dispatcher.Invoke(new System.Action(() => B_Output_2.Value += 25));
                            Dispatcher.Invoke(new System.Action(() => MUX_1.Value += 21.75));

                            Dispatcher.Invoke(new System.Action(() => move3 = new TranslateTransform(0, 281 + (B_Output_2.Value * 1.05))));
                            Dispatcher.Invoke(new System.Action(() => Register_Output_Content.RenderTransform = move3));

                            if (tempo == 25)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Hidden));
                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Text = Register_Output_Content.Text));

                        tempo = 0;

                        while (tempo != 100)//20
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => Out_In.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => MUX_2.Value  += 5));

                            Dispatcher.Invoke(new System.Action(() => move1 = new TranslateTransform((100 - Out_In.Value) * (3.44), 0)));
                            Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.RenderTransform = move1));

                            if (tempo == 5)
                            {
                                Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));
                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Text = Data_Bus_Content.Text));

                        tempo = 0;

                        while (tempo != 100)//2
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value += 50));
                            Dispatcher.Invoke(new System.Action(() => MUX_3.Value += 50));

                            Dispatcher.Invoke(new System.Action(() => move4 = new TranslateTransform(0, A_Input_1.Value * (-0.39))));
                            Dispatcher.Invoke(new System.Action(() => Register_Input_Content.RenderTransform = move4));

                            if (tempo == 50)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)//2
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value += 25));
                            Dispatcher.Invoke(new System.Action(() => MUX_4.Value += 50));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)//2
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value += 25));
                            Dispatcher.Invoke(new System.Action(() => B_Input_3.Value += 50));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 13://Move Data to X
                    {
                        local_timing /= 2;

                        RefreshDataBusContent(local_timing, 3);

                        Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));

                        RefreshRegisterInput(local_timing, 4);

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                default:
                    {
                        MessageBox.Show("Error in Perform Transfer " + type.ToString());
                        return;
                    }
            }
        }

        private void RefreshAluInput(bool type, int local_timing)
        {
            local_timing /= 120;

            int tempo = 0;

            while (tempo != 20)
            {
                Dispatcher.Invoke(new System.Action(() => A_Output_1.Value += 1));
                tempo++;

                if (type)
                {
                    Dispatcher.Invoke(new System.Action(() => B_Output_1.Value += 4.8));
                }

                System.Threading.Thread.Sleep(local_timing);

            }

            tempo = 0;

            while (tempo != 100)
            {
                Dispatcher.Invoke(new System.Action(() => MUX_1.Value += 1));
                tempo++;

                if (type)
                {
                    Dispatcher.Invoke(new System.Action(() => ALU_B.Value += 1));
                }

                System.Threading.Thread.Sleep(local_timing);
            }
        }

        private void RefreshOutIn(int local_timing)
        {
            Dispatcher.Invoke(new System.Action(() => Out_In.Visibility = Visibility.Visible));
            Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Text = Register_Output_Content.Text));

            local_timing /= 20;

            int tempo = 0;
            Dispatcher.Invoke(new System.Action(() => Activate_Bus()));

            while (tempo != 100)
            {
                tempo += 5;
                Dispatcher.Invoke(new System.Action(() => Out_In.Value += 5));
                Dispatcher.Invoke(new System.Action(() => move1 = new TranslateTransform((100 - tempo) * (3.44), 0)));
                Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.RenderTransform = move1));

                if (tempo == 5)
                {
                    Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Visible));
                }

                System.Threading.Thread.Sleep(local_timing);
            }
        }//move1 ---> verified

        private void RefreshRegisterInput(int local_timing, int type)
        {
            switch (type)
            {
                case 1://A
                    {
                        local_timing /= 6;

                        Dispatcher.Invoke(new System.Action(() => A_Input_1.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Text = Data_Bus_Content.Text ));

                        int tempo = 0;
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));

                        while (tempo != 100)
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => A_Input_1.Value += 50));
                            Dispatcher.Invoke(new System.Action(() => move4 = new TranslateTransform(0, tempo * (0.39))));
                            Dispatcher.Invoke(new System.Action(() => Register_Input_Content.RenderTransform = move4));

                            if (tempo == 50)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 25;
                            Dispatcher.Invoke(new System.Action(() => A_Input_2.Value += 25));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 2://B
                    {
                        local_timing /= 5;

                        Dispatcher.Invoke(new System.Action(() => B_Input_1.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Text = Data_Bus_Content.Text));

                        int tempo = 0;
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));

                        while (tempo != 100)
                        {
                            tempo += 25;
                            Dispatcher.Invoke(new System.Action(() => B_Input_1.Value += 25));
                            Dispatcher.Invoke(new System.Action(() => move4 = new TranslateTransform(0, tempo * (0.94))));
                            Dispatcher.Invoke(new System.Action(() => Register_Input_Content.RenderTransform = move4));

                            if (tempo == 25)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => B_Input_2.Value = 100));

                        System.Threading.Thread.Sleep(local_timing);

                        Dispatcher.Invoke(new System.Action(() => B_Input_3.Value = 100));

                        return;
                    }

                case 4://X
                    {
                        local_timing /= 14;

                        Dispatcher.Invoke(new System.Action(() => X_Input_1.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Text = Data_Bus_Content.Text));

                        int tempo = 0;
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));

                        while (tempo != 100)
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() => X_Input_1.Value += 10));
                            Dispatcher.Invoke(new System.Action(() => move4 = new TranslateTransform(0, tempo * (3.26))));
                            Dispatcher.Invoke(new System.Action(() => Register_Input_Content.RenderTransform = move4));

                            if (tempo == 10)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 25;
                            Dispatcher.Invoke(new System.Action(() => X_Input_2.Value += 25));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 5://Outport
                    {
                        local_timing /= 24;

                        Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Text = Data_Bus_Content.Text));

                        int tempo = 0;
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => In_Port_1.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => move4 = new TranslateTransform(0, tempo * (3.87))));
                            Dispatcher.Invoke(new System.Action(() => Register_Input_Content.RenderTransform = move4));

                            if (tempo == 5)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Input_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 25;
                            Dispatcher.Invoke(new System.Action(() => In_Port_2.Value += 25));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 6://Read & Address Bus simultanously
                    {
                        local_timing /= 43;

                        Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 100));

                        System.Threading.Thread.Sleep(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Text = Address_Register.Get().ToString("X3")));

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_1.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => move2 = new TranslateTransform(tempo * (2.40), 0)));
                            Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.RenderTransform = move2));

                            if (tempo == 5)
                            {
                                Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_2.Value += 5));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => RD_3.Value += 50));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Green));
                        Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.Pink));
                        Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 7://RW_clock
                    {
                        local_timing /= 21;//<--------+1

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 5));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Visible));

                        System.Threading.Thread.Sleep(local_timing);

                        return;
                    }

                case 8://Read & RW_clock
                    {
                        local_timing /= 42;

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_1.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 2));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_2.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 2));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => RD_3.Value += 50));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.Pink));
                        Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.LightGreen));

                        Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Visible));

                        return;
                    }

                case 9://RW_clock & Address Bus
                    {
                        local_timing /= 21;

                        Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 100));

                        System.Threading.Thread.Sleep(local_timing);

                        Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Text = Address_Register.Get().ToString("X3")));

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => move2 = new TranslateTransform(tempo * (2.40), 0)));
                            Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.RenderTransform = move2));

                            if (tempo == 5)
                            {
                                Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Show()));

                        Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                case 10://Read & HTemp & RW_clock
                    {
                        local_timing /= 43;//<--------+1

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_1.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 2));

                            if (tempo < 25)
                            {
                                Dispatcher.Invoke(new System.Action(() => HTemp_In_1.Value += 25));
                            }
                            else if (tempo == 25)
                            {
                                Dispatcher.Invoke(new System.Action(() => HTemp_In_2.Value = 100));
                            }
                            else if (tempo == 30)
                            {
                                Dispatcher.Invoke(new System.Action(() => HTemp_Register.Clock_Show()));
                            }
                            else if (tempo == 35)
                            {
                                Dispatcher.Invoke(new System.Action(() => HTemp_Register.Refresh()));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_2.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 2));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => RD_3.Value += 50));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.Pink));
                        Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.LightGreen));

                        Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Visible));

                        System.Threading.Thread.Sleep(local_timing);

                        return;
                    }

                case 11://RW_clock & Address Bus & Read
                    {
                        local_timing /= 44;//<-------------+1

                        Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Text = Address_Register.Get().ToString("X3")));
                        Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 100));

                        System.Threading.Thread.Sleep(local_timing);

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_1.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 2));

                            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => move2 = new TranslateTransform(tempo * (2.40), 0)));
                            Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.RenderTransform = move2));

                            if (tempo == 5)
                            {
                                Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Hidden));

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_2.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 2));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_3.Value += 50));
                            Dispatcher.Invoke(new System.Action(() => RW_clock.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.Pink));
                        Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.LightGreen));

                        Dispatcher.Invoke(new System.Action(() => Q_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Data_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => Address_Register.Clock_Show()));
                        Dispatcher.Invoke(new System.Action(() => WR_Control.Visibility = Visibility.Visible));

                        System.Threading.Thread.Sleep(local_timing);

                        return;
                    }

                default:
                    {
                        MessageBox.Show("Error in Register Input , unrecognized value " + type.ToString());
                        return;
                    }
            }


        }//move4 ---> verified

        private void RefreshRegisterOutput(int local_timing, int type)
        {
            switch (type)
            {
                case 1://A
                    {
                        local_timing /= 12;

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() => A_Output_1.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => A_Buffer.Fill = Brushes.LightGreen));
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));
                        Dispatcher.Invoke(new System.Action(() => A_Output_2.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Text = A_Register.Text));

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => A_Output_2.Value += 50));
                            Dispatcher.Invoke(new System.Action(() => move3 = new TranslateTransform(0, (-1) * ( 342 + tempo * 0.38))));
                            Dispatcher.Invoke(new System.Action(() => Register_Output_Content.RenderTransform = move3));

                            if (tempo == 50)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 2://B
                    {
                        local_timing /= 14;

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() => B_Output_1.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => B_Buffer.Fill = Brushes.LightGreen));
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));
                        Dispatcher.Invoke(new System.Action(() => B_Output_2.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Text = B_Register.Text));

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 25;
                            Dispatcher.Invoke(new System.Action(() => B_Output_2.Value += 25));
                            Dispatcher.Invoke(new System.Action(() => move3 = new TranslateTransform(0, (-1)* ( 281 + tempo * 1.05))));
                            Dispatcher.Invoke(new System.Action(() => Register_Output_Content.RenderTransform = move3));

                            if (tempo == 25)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 3://ALU
                    {
                        local_timing /= 7;

                        Dispatcher.Invoke(new System.Action(() => ALU_Output_1.Value = 100));
                        System.Threading.Thread.Sleep(local_timing);

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => ALU_Output_2.Value += 50));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => ALU_Buffer.Fill = Brushes.LightGreen));
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));
                        Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Text = A_Register.Get().ToString("X2")));

                        tempo = 0;
                        while (tempo != 100)
                        {
                            tempo += 20;
                            Dispatcher.Invoke(new System.Action(() => ALU_Output_3.Value += 20));//105
                            Dispatcher.Invoke(new System.Action(() => move3 = new TranslateTransform(0, (-1)* ( 115 + tempo * 2.76))));
                            Dispatcher.Invoke(new System.Action(() => Register_Output_Content.RenderTransform = move3));

                            if (tempo == 20)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        break;
                    }

                case 4://X
                    {
                        local_timing /= 20;

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() => X_Output_1.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => X_Buffer.Fill = Brushes.LightGreen));
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));
                        Dispatcher.Invoke(new System.Action(() => X_Output_2.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Text = X_Register.Text));

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() => X_Output_2.Value += 10));//55
                            Dispatcher.Invoke(new System.Action(() => move3 = new TranslateTransform(0, (-1)*(55 + X_Output_2.Value * 3.26))));
                            Dispatcher.Invoke(new System.Action(() => Register_Output_Content.RenderTransform = move3));

                            if (tempo == 10)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 5://Inport
                    {
                        local_timing /= 30;

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() => Out_Port_1.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Input_Buffer.Fill = Brushes.LightGreen));
                        Dispatcher.Invoke(new System.Action(() => Activate_Bus()));
                        //Out_Port_2.Visibility = Visibility.Visible -----> always visible
                        Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Text = In_Content.ToString("X2")));

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => Out_Port_2.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => move3 = new TranslateTransform(0, Out_Port_2.Value * -3.86)));
                            Dispatcher.Invoke(new System.Action(() => Register_Output_Content.RenderTransform = move3));

                            if (tempo == 5)
                            {
                                Dispatcher.Invoke(new System.Action(() => Register_Output_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 6://IR Output
                    {
                        local_timing /= 11;//<------+1

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() => IR_Output.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => ID.Clock_Show() ));

                        System.Threading.Thread.Sleep(local_timing);

                        break;
                    }

                case 7://Decoder Output
                    {
                        local_timing /= 2;

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => Decoder_Output.Value += 50));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 8://fisrt part of LDA & STA address modification
                    {
                        local_timing /= 9;

                        int tempo = 0;

                        while (tempo != 100)//4
                        {
                            tempo += 25;
                            Dispatcher.Invoke(new System.Action(() => LTemp_Output.Value += 25));
                            Dispatcher.Invoke(new System.Action(() => HTemp_Output.Value += 25));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)//5
                        {
                            tempo += 20;
                            Dispatcher.Invoke(new System.Action(() => Temp_Output.Value += 20));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 9://Read
                    {
                        local_timing /= 42;

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_1.Value += 5));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => RD_2.Value += 5));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 50;
                            Dispatcher.Invoke(new System.Action(() => RD_3.Value += 50));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Q_Buffer.Fill = Brushes.Pink));
                        Dispatcher.Invoke(new System.Action(() => Data_Buffer.Fill = Brushes.LightGreen));

                        return;
                    }

                case 10://WR
                    {
                        local_timing /= 20;

                        int tempo = 0;

                        while (tempo != 100)
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => WR.Value += 5));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 11://Successful Jump address modification
                    {
                        local_timing /= 16;

                        int tempo = 0;

                        while (tempo != 100)//4
                        {
                            tempo += 25;
                            Dispatcher.Invoke(new System.Action(() => LTemp_Output.Value += 25));
                            Dispatcher.Invoke(new System.Action(() => HTemp_Output.Value += 25));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 30)//2
                        {
                            tempo += 15;
                            Dispatcher.Invoke(new System.Action(() => Temp_Output.Value += 15));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        tempo = 0;

                        while (tempo != 100)//10
                        {
                            tempo += 10;
                            Dispatcher.Invoke(new System.Action(() => PC_In.Value += 10));
                            System.Threading.Thread.Sleep(local_timing);
                        }

                        return;
                    }

                case 12://second part of LDA & STA address modification
                    {
                        local_timing /= 20;

                        int tempo = 0;

                        Dispatcher.Invoke(new System.Action(() => MUX_Address_0.Fill = Brushes.Red));
                        Dispatcher.Invoke(new System.Action(() => MUX_Address_1.Fill = Brushes.LightGreen));
                        Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Text = Address_Register.Get().ToString("X3")));

                        while (tempo != 100)//20
                        {
                            tempo += 5;
                            Dispatcher.Invoke(new System.Action(() => Address_Bus.Value += 5));
                            Dispatcher.Invoke(new System.Action(() => move2 = new TranslateTransform(tempo * (2.40), 0)));
                            Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.RenderTransform = move2));

                            if (tempo == 5)
                            {
                                Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Visible));
                            }

                            System.Threading.Thread.Sleep(local_timing);
                        }

                        Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Hidden));

                        return;
                    }

                default:
                    {
                        MessageBox.Show("Error in Register Output , unrecognized value " + type.ToString());
                        return;
                    }
            }
        }//move3 ---> verified

        private void RefreshDataBusContent(int local_timing)
        {
            local_timing /= 29;//<----28;

            Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Text = A_Register.Text));
            Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_1.Visibility = Visibility.Visible));

            int tempo = 0;
            Dispatcher.Invoke(new System.Action(() => Activate_Bus()));

            while (tempo != 100)//20
            {
                tempo += 5;
                Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_1.Value += 5));
                Dispatcher.Invoke(new System.Action(() => WR.Value += 4.5));
                Dispatcher.Invoke(new System.Action(() => move1 = new TranslateTransform(356 + (tempo * 5.08), 0)));
                Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.RenderTransform = move1));

                if (tempo == 5)
                {
                    Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Visible));
                }

                Thread.Sleep(local_timing);
            }

            Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Hidden));
            Thread.Sleep(local_timing);

            tempo = 0;

            while (tempo != 100)//4
            {
                tempo += 25;
                Dispatcher.Invoke(new System.Action(() => WR.Value += 5));
                Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_2.Value += 25));
                Thread.Sleep(local_timing);
            }

            tempo = 0;

            while (tempo != 100)//4
            {
                tempo += 25;
                Dispatcher.Invoke(new System.Action(() => WR.Value += 5));
                Dispatcher.Invoke(new System.Action(() => Data_Bus_WR_3.Value += 25));
                Thread.Sleep(local_timing);
            }

            return;

        }//move1 ---> verified

        private void RefreshDataBusContent(int local_timing, int type)
        {
            Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Text = Data_Register.Text));

            int value = 0;

            switch (type)
            {
                case 1:
                    {
                        value = 45;
                        break;
                    }

                case 2:
                    {
                        value = 54;
                        break;
                    }

                case 3:
                    {
                        value = 100;
                        break;
                    }

                default:
                    {
                        MessageBox.Show("Error in Data Bus Read");
                        break;
                    }
            }

            local_timing /= value;

            int tempo = 0;
            Dispatcher.Invoke(new System.Action(() => Activate_Bus()));

            while (tempo != value)
            {
                tempo += 1;
                Dispatcher.Invoke(new System.Action(() => Data_Bus.Value += 1));
                Dispatcher.Invoke(new System.Action(() => move1 = new TranslateTransform((100 - tempo) * (8.95), 0)));
                Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.RenderTransform = move1));

                if (tempo == 1)
                {
                    Dispatcher.Invoke(new System.Action(() => Data_Bus_Content.Visibility = Visibility.Visible));
                }

                System.Threading.Thread.Sleep(local_timing);
            }

            return;

        }//move1 ---> verified

        private void RefreshAddressBusContent(int local_timing)
        {
            local_timing /= 22;//<---21;

            Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Text = Address_Register.Get().ToString("X3")));

            Dispatcher.Invoke(new System.Action(() => PC_Output.Value = 100));

            Thread.Sleep(local_timing);

            int tempo = 0;

            while (tempo != 100)
            {
                tempo += 5;
                Dispatcher.Invoke(new System.Action(() => Address_Bus.Value += 5));
                Dispatcher.Invoke(new System.Action(() => move2 = new TranslateTransform(tempo * (2.40), 0)));
                Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.RenderTransform = move2));

                if (tempo == 5)
                {
                    Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Visible));
                }

                Thread.Sleep(local_timing);
            }

            Dispatcher.Invoke(new System.Action(() => Address_Bus_Content.Visibility = Visibility.Hidden));

            Thread.Sleep(local_timing);

            return;

        }//move2 ---> verified

        private void RefreshState()
        {
            string temp1 = "";
            string temp2 = "";

            if (Address_Register.Get() == 0)
            {
                temp1 = "---";
                Dispatcher.Invoke(new System.Action(() => temp2 = (string)RAM.Items[0]));
            }
            else
            {
                Dispatcher.Invoke(new System.Action(() => temp1 = (string)RAM.Items[Address_Register.Get() - 1]));
                Dispatcher.Invoke(new System.Action(() => temp2 = (string)RAM.Items[Address_Register.Get()]));

                if (temp1.Contains("HLT"))
                {
                    temp2 = "---";
                }

                if (temp2.Contains("2nd Byte"))
                {
                    Dispatcher.Invoke(new System.Action(() => temp2 = (string)RAM.Items[Address_Register.Get() + 1]));
                }
            }

            Dispatcher.Invoke(new System.Action(() => State.SetState(temp1, temp2)));
        }

        private Info Set_Info()
        {
            Last_Info = new Info();

            Last_Info.C_Flag = C_Flag.Get();
            Last_Info.O_Flag = O_Flag.Get();
            Last_Info.N_Flag = N_Flag.Get();
            Last_Info.Z_Flag = Z_Flag.Get();

            Last_Info.A_Content = A_Register.Get();
            Last_Info.B_Content = B_Register.Get();
            Last_Info.X_Content = X_Register.Get();
            
            Last_Info.Data_Content = Data_Register.Get();
            Last_Info.Address_Content = Address_Register.Get();

            Last_Info.IR_Content = IR_Register.Get();
            Last_Info.HTemp_Content = HTemp_Register.Get();
            Last_Info.LTemp_Content = LTemp_Register.Get();
            Last_Info.PC_Content = PC_Register.Get();

            Last_Info.Out_Content = Output_Register.Get();
            Dispatcher.Invoke(new System.Action(() => Last_Info.In_Content = (byte)Input_Register.SelectedIndex));//to be checked in future

            for (int i = 0; i < 4096; i++)
            {
                Last_Info.RAM_Content[i] = Ablauf[i];
            }

            return Last_Info;
        }

        private void Get_Info(int address)
        {
            for (int i = 0 ; i < 4096; i++)
            {
                if (Ablauf[i] != Sim_Info[address].RAM_Content[i])
                {
                    Ablauf[i] = Sim_Info[address].RAM_Content[i];
                    ModifyRamContent(i);
                }
                else
                {
                    Ablauf[i] = Sim_Info[address].RAM_Content[i];
                }
            }

            Dispatcher.Invoke(new System.Action(() => C_Flag.Refresh(Sim_Info[address].C_Flag)));
            Dispatcher.Invoke(new System.Action(() => O_Flag.Refresh(Sim_Info[address].O_Flag)));
            Dispatcher.Invoke(new System.Action(() => N_Flag.Refresh(Sim_Info[address].N_Flag)));
            Dispatcher.Invoke(new System.Action(() => Z_Flag.Refresh(Sim_Info[address].Z_Flag)));

            Dispatcher.Invoke(new System.Action(() => A_Register.Refresh(Sim_Info[address].A_Content)));
            Dispatcher.Invoke(new System.Action(() => B_Register.Refresh(Sim_Info[address].B_Content)));
            Dispatcher.Invoke(new System.Action(() => X_Register.Refresh(Sim_Info[address].X_Content)));
            
            Dispatcher.Invoke(new System.Action(() => Q_Register.Unknown()));
            Dispatcher.Invoke(new System.Action(() => Data_Register.Refresh(Sim_Info[address].Data_Content)));
            Dispatcher.Invoke(new System.Action(() => Address_Register.Refresh(Sim_Info[address].Address_Content)));

            Dispatcher.Invoke(new System.Action(() => IR_Register.Refresh(Sim_Info[address].IR_Content)));
            Dispatcher.Invoke(new System.Action(() => HTemp_Register.Refresh(Sim_Info[address].HTemp_Content)));
            Dispatcher.Invoke(new System.Action(() => LTemp_Register.Refresh(Sim_Info[address].LTemp_Content)));
            Dispatcher.Invoke(new System.Action(() => PC_Register.Refresh(Sim_Info[address].PC_Content)));
            Dispatcher.Invoke(new System.Action(() => ID.Set(IR_Register.Get() , N_Flag.Get(), Z_Flag.Get(), O_Flag.Get(), C_Flag.Get())));

            Dispatcher.Invoke(new System.Action(() => Output_Register.Refresh(Sim_Info[address].Out_Content)));
            Dispatcher.Invoke(new System.Action(() => Input_Register.SelectedIndex = Sim_Info[address].In_Content));
        }

        private void Reset()
        {
            State.SetState("", "");
            State.SetMessage("");

            simulation.Abort();

            N_Flag.Reset();
            Z_Flag.Reset();
            PC_Register.Reset();

            N_Flag.Refresh();
            Z_Flag.Refresh();
            PC_Register.Refresh();

            //Data_Register.Refresh(Ablauf[0]);
            //Q_Register.Unknown();
            //Data_Register.Unknown();

            //for (int i = 0; i < 4096; i++)
            //{
            //    Ablauf[i] = Ablauf_Backup[i];
            //}

            //RAM.Items.Clear();

            //foreach (string item in Description_Backup)
            //{
            //    RAM.Items.Add(item);
            //}

            RAM.SelectedIndex = 0;
            simulation = new System.Threading.Thread(Simulate);

            MessageBox.Show("Processor resetted");

            simulation.Start();

            N_Flag.Reset_Hide();
            Z_Flag.Reset_Hide();
            PC_Register.Reset_Hide();

            clk_enable.Background = Brushes.Green;
            Center.Background = (ImageBrush)FindResource("Pause");
        }

        private void LoadNextInstruction()
        {
            Skip = false;
            animation_on = true;
            Get_Info(Iselect);
        }

        private string GetMappedAddress(byte address)
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
                        if (Z_Flag.Get())
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
                        if (!Z_Flag.Get())
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
                        if (C_Flag.Get())
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
                        if (!C_Flag.Get())
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
                        if (N_Flag.Get())
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
                        if (!N_Flag.Get())
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
                        if (O_Flag.Get())
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

        //Event Handlers

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Tselect++;
            State.SetMessage("Seeking forward to " + Tselect + ".cycle");
        }

        private void Center_Click(object sender, RoutedEventArgs e)
        {
            if     (simulation.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                //marque.Start();
                simulation.Start();
                Center.Background = (ImageBrush)FindResource("Pause");
                Center.ToolTip = "Suspend Simulation";
            }
            else if (simulation.ThreadState == System.Threading.ThreadState.WaitSleepJoin )
            {
                simulation.Suspend();
                Center.Background = (ImageBrush)FindResource("Play");
                Center.ToolTip = "Resume Simulation";
            }
            else if (simulation.ThreadState == System.Threading.ThreadState.Suspended)
            {
                simulation.Resume();
                Center.Background = (ImageBrush)FindResource("Pause");
                Center.ToolTip = "Suspend Simulation";
            }
            else if (simulation.ThreadState == System.Threading.ThreadState.Aborted)
            {
                MessageBox.Show("Nothing to do , to run simulation again reset processor");
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (move_requested)
            {
                return;
            }

            if (returntofirst)
            {
                MessageBox.Show("Unable to move back simulation in first fetch");
            }

            if (Tcycle > 2)
            {
                Tselect-=2;
            }
            else
            {
                Tselect=0;
            }

            move_requested = true;
            Iselect = start_address;
            State.SetMessage("Going back to " + Tselect + ".cycle");
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (simulation.ThreadState == ThreadState.Running)
            {
                reset = true;
            }
            else
            {
                Dispatcher.Invoke(new System.Action(() => N_Flag.Reset_Show()));
                Dispatcher.Invoke(new System.Action(() => Z_Flag.Reset_Show()));
                Dispatcher.Invoke(new System.Action(() => PC_Register.Reset_Show()));
                System.Threading.Thread.Sleep(timing);

                Reset();
            } 
        }

        private void ALU_Status_MouseEnter(object sender, MouseEventArgs e)
        {
            signals.Visibility = Visibility.Hidden;
        }

        private void ALU_Status_MouseLeave(object sender, MouseEventArgs e)
        {
            signals.Visibility = Visibility.Visible;
        }

        private void RAM_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RAM.SelectedIndex == -1)
            {
                MessageBox.Show("No instruction is selected");
                return;
            }
            
            if (Sim_Info[RAM.SelectedIndex] != null)
            {
                if (((string)RAM.SelectedItem).LastIndexOf("2nd Byte") == -1)
                {
                    animation_on = false;
                    Skip = true;
                    Iselect = RAM.SelectedIndex;
                    State.SetNext((string)RAM.SelectedItem);
                    MessageBox.Show("Simulator will move simulation to selected instruction after next fetch");
                }
                else
                {
                    MessageBox.Show("Simulation can't be moved to 2nd Byte instruction");
                }
            }
            else
            {
                simulation.Suspend();

                TimeAnalyzer analyzer = new TimeAnalyzer();
                Queue inputs = new Queue();
                AnalyzeResult res = analyzer.Analyze(inputs, Sim_Info[0] , RAM.SelectedIndex);


                if (res.NumberOfINS == -1)
                {
                    MessageBox.Show("Analyzer discovered infinte loop , simulation can't be moved selected address");
                    return;
                }
                else
                {
                    if (res.NumberOfINA != 0)
                    {
                        string result = Microsoft.VisualBasic.Interaction.InputBox("Analyzer discovered " + res.NumberOfINA.ToString() + " INA instructions , to run analyzer with specific input values enter them below , click on Cancel to run analyzer with default 0 value", "Input", "Default data", -1, -1);

                        if (!String.IsNullOrWhiteSpace(result))
                        {
                            string[] input_values = result.Split(trim4, StringSplitOptions.RemoveEmptyEntries);
                            inputs.Clear();
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

                                    inputs.Enqueue(counter3);

                                }
                                catch
                                {
                                    inputs.Enqueue(0);
                                }
                            }

                            res = analyzer.Analyze(inputs , Sim_Info[0] , RAM.SelectedIndex);
                        }
                    }
                }

                Skip = true;
                animation_on = false;

                Iselect = RAM.SelectedIndex;
                Sim_Info[Iselect] = res.Memory;
                State.SetNext((string)RAM.SelectedItem);

                MessageBox.Show("Simulator will move simulation to selected instruction after next fetch");
                simulation.Resume();
            }
        }

        private void Simulator_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (simulation.ThreadState != ThreadState.Stopped & simulation.ThreadState != ThreadState.Suspended)
            {
                simulation.Abort();
            }

            N_Flag.Close();
            Z_Flag.Close();
            O_Flag.Close();
            C_Flag.Close();

            A_Register.Close();
            B_Register.Close();
            X_Register.Close();

            ID.Close();
            IR_Register.Close();
            PC_Register.Close();

            Q_Register.Close();
            Data_Register.Close();
            Address_Register.Close();
            
            Output_Register.Close();
            
            LTemp_Register.Close();
            HTemp_Register.Close();

        }

        private void Activate_Bus()
        {
            Out_In.Background        = Brushes.GreenYellow;
            IR_Input.Background      = Brushes.GreenYellow;
            Data_Bus.Background      = Brushes.GreenYellow;
            In_Port_1.Background     = Brushes.GreenYellow;
            A_Input_1.Background     = Brushes.GreenYellow;
            A_Input_2.Background     = Brushes.GreenYellow;
            B_Input_1.Background     = Brushes.GreenYellow;
            B_Input_2.Background     = Brushes.GreenYellow;
            X_Input_1.Background     = Brushes.GreenYellow;
            X_Input_2.Background     = Brushes.GreenYellow;
            In_Port_2.Background     = Brushes.GreenYellow;
            Out_Port_2.Background    = Brushes.GreenYellow;
            LTemp_Input.Background   = Brushes.GreenYellow;
            Data_Bus_WR_1.Background = Brushes.GreenYellow;
            Data_Bus_WR_2.Background = Brushes.GreenYellow;
            Data_Bus_WR_3.Background = Brushes.GreenYellow;

            A_Output_2.Background = Brushes.GreenYellow;
            B_Output_2.Background = Brushes.GreenYellow;
            X_Output_2.Background = Brushes.GreenYellow;
        }

        private void Deactivate()
        {
            Out_In.Background        = Brushes.LightGray;
            IR_Input.Background      = Brushes.LightGray;
            Data_Bus.Background      = Brushes.LightGray;
            In_Port_1.Background     = Brushes.LightGray;
            A_Input_1.Background     = Brushes.LightGray;
            A_Input_2.Background     = Brushes.LightGray;
            B_Input_1.Background     = Brushes.LightGray;
            B_Input_2.Background     = Brushes.LightGray;
            X_Input_1.Background     = Brushes.LightGray;
            X_Input_2.Background     = Brushes.LightGray;
            In_Port_2.Background     = Brushes.LightGray;
            Out_Port_2.Background    = Brushes.LightGray;
            LTemp_Input.Background   = Brushes.LightGray;
            Data_Bus_WR_1.Background = Brushes.LightGray;
            Data_Bus_WR_2.Background = Brushes.LightGray;
            Data_Bus_WR_3.Background = Brushes.LightGray;

            A_Output_2.Background = Brushes.LightGray;
            B_Output_2.Background = Brushes.LightGray;
            X_Output_2.Background = Brushes.LightGray;
        }
    }
}
