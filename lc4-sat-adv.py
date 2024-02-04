# Copyright (c) Microsoft Corporation 2015, 2016

# The Z3 Python API requires libz3.dll/.so/.dylib in the 
# PATH/LD_LIBRARY_PATH/DYLD_LIBRARY_PATH
# environment variable and the PYTHONPATH environment variable
# needs to point to the `python' directory that contains `z3/z3.py'
# (which is at bin/python in our binary releases).

# If you obtained example.py as part of our binary release zip files,
# which you unzipped into a directory called `MYZ3', then follow these
# instructions to run the example:

# Running this example on Windows:
# set PATH=%PATH%;MYZ3\bin
# set PYTHONPATH=MYZ3\bin\python
# python example.py

# Running this example on Linux:
# export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:MYZ3/bin
# export PYTHONPATH=MYZ3/bin/python
# python example.py

# Running this example on macOS:
# export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:MYZ3/bin
# export PYTHONPATH=MYZ3/bin/python
# python example.py


from z3 import *

def SelectAt(vector, pos):
    return If(pos==0, vector[0],
              If(pos==1, vector[1],
                 If(pos==2, vector[2],
                    If(pos==3, vector[3],
                       If(pos==4, vector[4],
                          If(pos==5, vector[5],
                             If(pos==6, vector[6],
                                If(pos==7, vector[7],
                                   If(pos==8, vector[8],
                                      If(pos==9, vector[9],
                                         If(pos==10, vector[10],
                                            If(pos==11, vector[11],
                                               If(pos==12, vector[12],
                                                  If(pos==13, vector[13],
                                                     If(pos==14, vector[14],
                                                        If(pos==15, vector[15],
                                                           If(pos==16, vector[16],
                                                              If(pos==17, vector[17],
                                                                 If(pos==18, vector[18],
                                                                    If(pos==19, vector[19],
                                                                       If(pos==20, vector[20],
                       If(pos==21, vector[21],
                            If(pos==22, vector[22],
                                If(pos==23, vector[23],
                                   If(pos==24, vector[24],
                                       If(pos==25, vector[25],
                                         If(pos==26, vector[26],
                                           If(pos==27, vector[27],
                                              If(pos==28, vector[28],
                                               If(pos==29, vector[29],
                                                     If(pos==30, vector[30],
                                                        If(pos==31, vector[31],
                                                            If(pos==32, vector[32],
                                                              If(pos==33, vector[33],
                                                                If(pos==34, vector[34],
                                                                          If(pos==35, vector[35],
                                                                          -1
                                                                          
              )
                                                                          
              )
              )          
              )            
              )              
              )                  
              )                     
              )                           
              )                            
              )                               
              )                                 
              )                                   
              )                                       
              )                                          
              )                                              
              )                                                   
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )
              )





cipher = [22,2,5,7,9,30,12,14,22,24,28,32,15,1,3,23,16,8,23,10,32,18,5,7,2,5,17,14,22,3,22,22,9,5,33,26,10,10,30,35,12,23,0,16,21,31,33,31,11,0,26,5,9,25,19,19]#,24,16,22,18,25,14,6,30,8,12,29,21,16,9,5,6,25,7,24,28,25,1,18,29,22,21,27,30,6,34,34,28,13,9,33,15,12,10,26,0,23,0,35,11,28,31,6,28,24,21,29,31,15,7,1,29,22,0,31,29,0,21,35,20,35,34,3,7,9,3,22,9,32,8,27,5,16,12,33,10,17,32,4,3,20,15,26,1,24,2,12,34,14,30,11,0,17,27,5,32,13,32,10,33,29,29,6,28,14,1,14,35,20,14,10,16,0,32,17,15,29,18,2,10,14,7,18,25,8,23,0,34,26,23,1,18,19,6,10,14,27,30,15,27]
#[1,2,3]

clear = [11,19,35,7,8,5,7,6,6,32,26,2,21,13,31,10,4,27,26,22,12,35,11,25,31,23,4,24,19,25,3,26,22,23,16,11,16,23,2,0,28,15,5,25,12,21,12,15,5,32,8,20,0,20,3,18,2,6,27,31,2,11,26,30,11,2,3,31,27,34,6,18,13,15,30,3,26,30,9,14,32,4,29,3,32,26,32,9,31,34,4,0,20,11,25,17,10,32,4,9,15,11,4,1,18,12,31,26,3,35,22,19,26,19,27,24,35,24,6,9,25,19,33,3,26,9,24,10,19,34,29,23,21,11,15,12,32,18,26,2,6,24,7,25,16,35,8,17,8,30,3,8,19,13,30,24,29,31,30,5,18,18,2,11,6,32,4,14,5,14,14,10,30,20,6,23,19,34,4,28,28,29,21,4,8,29,1,14,28,27,9,24,28,26,31,0,34,9,29,19]
#[3,4,5]

s = Solver()

#State = Int('key_'+identifier)[6][6]

A = Array('A', IntSort(), ArraySort(IntSort(), IntSort()))
x, y = Ints('x y')
print(A[x][y])

#if some key positions are known:
#s.add(State[0]==4)
#s.add(State[1]==20)
#s.add(State[2]==23)
#s.add(State[3]==1)
#s.add(State[4]==5)
#s.add(State[5]==25)

#s.add(State[6]==6)
#s.add(State[7]==14)
#s.add(State[8]==7)
#s.add(State[9]==33)
#s.add(State[10]==9)
#s.add(State[11]==3)

#s.add(State[12]==11)
#s.add(State[13]==18)
#s.add(State[14]==12)
#s.add(State[15]==8)
#s.add(State[16]==22)
#s.add(State[17]==24)

#s.add(State[18]==35)
#s.add(State[19]==31)
#s.add(State[20]==13)
#s.add(State[21]==15)
#s.add(State[22]==19)
#s.add(State[23]==16)

#s.add(State[24]==0)
#s.add(State[25]==21)
#s.add(State[26]==10)
#s.add(State[27]==28)
#s.add(State[28]==26)
#s.add(State[29]==32)

#s.add(State[30]==17)
#s.add(State[31]==30)
#s.add(State[32]==34)
#s.add(State[33]==27)
#s.add(State[34]==29)
#s.add(State[35]==2)


s.add(Distinct(State))
for i in range(0,36):
    s.add(State[i]>=0)
    s.add(State[i]<36)


           




def lc4round(State, index0, cipherValue, clearValue, identifier):
    pos1 = Int('pos1_'+identifier)
    s.add(pos1==SelectAt(State, index0))
    pos_r = Int('posr_'+identifier)
    s.add(pos_r == pos1%6)
    pos_d = Int('posd_'+identifier)
    s.add(pos_d == pos1/6)
    pos_clear = Int('pos_cl_'+identifier)
    s.add(pos_clear >= 0)
    s.add(pos_clear < 36)

    s.add(clearValue==SelectAt(State, pos_clear))
    pos_cipher = Int('pos_c_'+identifier)
    s.add(pos_cipher >= 0)
    s.add(pos_cipher < 36)
    #s.add(pos_cipher==(pos_clear+pos_r+6*pos_d)%36)
    s.add(pos_cipher == ((pos_clear/6+pos_d)%6)*6 + (pos_clear%6+pos_r)%6)  
    s.add(cipherValue==SelectAt(State, pos_cipher))

    #update state

    column_cipher = Int('column_c_'+identifier)
    s.add(column_cipher == pos_cipher%6)
    row_cipher = Int('row_c_'+identifier)
    s.add(row_cipher == pos_cipher/6)
    row_clear = Int('row_clear_'+identifier)
    s.add(row_clear == pos_clear/6)

    #rotate row right
    State2_r = IntVector("s2_"+identifier, 36)
    for i in range(0,36):
        s.add(State2_r[i]==If(i//6==row_clear, SelectAt(State, row_clear*6+(i+5)%6), State[i]))
    #if index was moved by rotation:
    index_r1 = Int('index_'+identifier)
    #s.add(index_r1==If(index0/6==row_clear, (index0+1)%36, index0))
    s.add(index_r1==If(index0/6==row_clear, (index0/6)*6+(index0+1)%6, index0))

    column_cipher_r = Int('column_cipher_r_'+identifier)
    s.add(column_cipher_r==If(row_cipher==row_clear, (column_cipher+1)%6, column_cipher))

    #rotate column down
    State2_r2 = IntVector("s3_"+identifier, 36)
    for i in range(0,36):
        s.add(State2_r2[i]==If(i%6==column_cipher_r, SelectAt(State2_r, (i+30)%36), State2_r[i]))
    #if index was moved by rotation:
    index_r2 = Int('index2_'+identifier)
    s.add(index_r2==If(index_r1%6==column_cipher_r, (index_r1+6)%36, index_r1))

    #update index

    cipherMoveR = Int('cipherR_'+identifier)
    s.add(cipherMoveR == cipherValue%6)

    cipherMoveD = Int('cipherD2_'+identifier)
    s.add(cipherMoveD == cipherValue//6)

    index = Int('index3_'+identifier)
    s.add(index == ((index_r2/6+cipherMoveD)%6)*6 + (index_r2%6+cipherMoveR)%6)  
    #rows*6 +rows
    return State2_r2, index
    #repeat


#loop:
VarState = [None] * (len(cipher)+1)
VarState[0] = State
varIndex = [None] * (len(cipher)+1)
varIndex[0] = index0
for i in range(0,len(cipher)):
    VarState[i+1], varIndex[i+1] = lc4round(VarState[i], varIndex[i], cipher[i], clear[i], str(i))



print(s.check())
model = s.model()
print(model)

#for i in range(0,2):
#    f = open(str(i)+"info.txt", "a")
#    f.write(model["posr"+str(i)]+"\n")
#    f.write(model["posd"+str(i)]+"\n")
#    f.close()

#for i in range(0,36):
#    print(i, ": ", model["s30__"+str(i)])

#print("------")
#print(model["index30"])

for k in range(0,len(cipher)+1):
    var = "{"
    print("------")
    for i in range(0,36):
        var += ","+str(model[VarState[k][i]])
    var+="}"
    print("------")
    print(model[varIndex[k]])
    print("------")
    print(var)
    



#for i in range(0,36):
#    print(model[State[i]],"; ",model[State2_r[i]])

#print("----------------------")
#for i in range(0,36):
#    print(model[State2_r[i]],"; ",model[State2_r2[i]])
