
import math

for n in range(5,26):
    
    a = n
    
    for i in range(1,(n-1)//2+1):
        a += i*(n//(2*i))+(n%i)*((n//i)%2)
        
    print(n,": ",a, ": ",(a/2)/n)