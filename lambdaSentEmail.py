from Emails.SendEmail import *
import json
def sent(event=None, context=None):
     
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
    if (SendingEmail(data) == 'Sent successfully'):
        return {
                'statusCode': 200,
                'headers': {'Content-Type': 'application/json'},
                'body' : data
            }
    return {
            'statusCode': 200,
            'headers': {'Content-Type': 'application/json'},
            'body': json.dumps('Sent faild')
        }    