import smtplib

# Document:
# https://www.courier.com/blog/three-ways-to-send-emails-using-python-with-code-tutorials/
def sendEmail(data):
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
        return "Sent successfully"

    except Exception as ex: 
        print("Something went wrong....", ex)