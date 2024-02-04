import math





def A(nBase, i, n):
     if i==0:
         return nBase**(n-4)
     else:
         subtract = 0
         for j in range(0,i-1):
             subtract+= int(A(nBase, j, n)/(nBase**2))

         return nBase**(n-4) - int(A(nBase, i-1, n)/nBase) - subtract

def A_new(array, nBase, i, n):
    if i==0:
        array[0]=nBase**(n-4)
    else:
        subtract = 0
        for j in range(0,i-1):
            subtract+= int(array[j]/(nBase**2))
            #subtract+= int(A(nBase, j, n)/(nBase**2))

        array[i]=nBase**(n-4) - int(array[i-1]/nBase) - subtract
       



#i_array = [-1]*len(msg)

def I(i_array, a_array, msg, nBase, k, n):
    #print("k-1:", msg[k-1])
    subtract = 0
    for i in range(0,k-1):
        #subtract+= int(I(nBase, i, n)/(nBase**2))
        subtract+= int(i_array[i]/(nBase**2))

    if k==0:
        if msg[0]!=0:
             i_array[k]=nBase**(n-1)#for faster execution
             return nBase**(n-1)
        else:
             i_array[k]=(2*nBase-2)*nBase**(n-1)#for faster execution
             return (2*nBase-2)*nBase**(n-1)
    elif msg[k]!=0 and msg[k-1]!=0:
        i_array[k]=nBase**(n-1) - int(i_array[k-1]/nBase) - subtract#for faster execution
        return i_array[k]#nBase**(n-1) - int(I(nBase, k-1, n)/nBase) - subtract
    elif msg[k]==0:
        i_array[k]=(2*nBase-2)*nBase**(n-1) - (nBase-1)*int(i_array[k-1]/nBase) - (2*nBase-2)*subtract#for faster execution
        return i_array[k]#(2*nBase-2)*nBase**(n-1) - (nBase-1)*int(I(nBase, k-1, n)/nBase) - (2*nBase-2)*subtract
    elif msg[k-1]==0:
        
        subtract2 = 0
        for i in range(0,k-2):
            #subtract2+= A(nBase, i, n)
            subtract2+= a_array[i]#A(nBase, i, n)
            #if(a_array[i]!=A(nBase, i, n)):
            #    print(a_array[i],":",i,":",A(nBase, i, n))
        #print("sub: ", subtract2)
        star = (nBase-1)*(nBase**(n-2)-subtract2)
        #print("star: ", star)
        i_array[k]=nBase**(n-1) - star - subtract#for faster execution
        return nBase**(n-1) - star - subtract

def omega(msgLength, zeroPos, nBase):

    arr = [-1]*msgLength

    for i in range(0,msgLength):
        A_new(arr,nBase,i,msgLength-1)
        #print(arr[i], ":", A(36,i,36))

    msg = [1] * msgLength#[0,1,1,1,1,1,1,1]
    msg[zeroPos]=0
    i_array = [-1]*len(msg)
    imposs = 0
    for i in range(0,len(msg)-1):
        round = I(i_array, arr, msg, nBase, i, len(msg)-1)
        imposs += round
    return imposs

def omega_star(msgLength, nBase):

    arr = [-1]*msgLength

    for i in range(0,msgLength):
        A_new(arr,nBase,i,msgLength-1)
        #print(arr[i], ":", A(36,i,36))

    msg = [1] * msgLength
    i_array = [-1]*len(msg)
    imposs = 0
    for i in range(0,len(msg)-1):
        round = I(i_array, arr, msg, nBase, i, len(msg)-1)
        imposs += round
    return imposs





# for i in range(0,36):
#     var = omega(36,i,36)
#     p = var/36**36
#     print(str(i)+" & "+str(var)+" & "+str(p)+"\\\\")

for length in range(2,37):
    sum = 0
    for i in range(0,length):
        sum += omega(length,i,36)
    if length==2:
        print(sum)
    var = omega_star(length,36)
    r = (length/36)*(sum/length)+((36-length)/36)*var
    p = r/(36**length)
    print(str(length)+" & "+str(r)+" & "+str(p)+"\\\\")


#print(omega_star(2,36))




