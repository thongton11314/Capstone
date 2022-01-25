import smtplib

# Document:
# https://www.courier.com/blog/three-ways-to-send-emails-using-python-with-code-tutorials/
def SendingEmail(data):
    try: 
        #Create your SMTP session 
        smtp = smtplib.SMTP('smtp.gmail.com', 587) 

        #Use TLS to add security 
        smtp.starttls() 

        #User Authentication 
        smtp.login(data['senderEmail'], data['senderEmailPassword'])

        #Sending the Email
        smtp.sendmail(data['senderEmail'], data['receiverEmail'], data['message']) 

        #Terminating the session 
        smtp.quit() 
        print ("Email sent successfully!") 

    except Exception as ex: 
        print("Something went wrong....", ex)

        
senderEmail = 'sinhvienngheouw@gmail.com'
senderPassword = 'Ngheo123456.'
receiverEmail = 'tonthong@uw.edu'
message = 'This is a test from auto sending email from Pythoncode'
data = {
    'senderEmail' : senderEmail, 
    'senderEmailPassword' : senderPassword,
    'receiverEmail' : receiverEmail,
    'message' : message
}
SendingEmail(data)