Dim objHTTP, domPage

Dim strResponse 
strResponse = TestPreview()
MsgBox strResponse

Function TestPreview()

    Set domPage = WScript.CreateObject("MSXML2.DomDocument")
    domPage.Load "page.xml"

    Set objHTTP = WScript.CreateObject("Microsoft.XMLHTTP")
    Rem objHTTP.open "GET", "http://localhost:9991/", False
    objHTTP.open "POST", "http://dev.staging.my.com", False
 
    objHTTP.setRequestHeader "Content-Type", "application/x-www-form-urlencoded"
    objHTTP.send domPage.Xml
    Rem objHTTP.send 

    TestPreview = objHTTP.ResponseText
End Function

Function TestSearch()

    Set objHTTP = WScript.CreateObject("Microsoft.XMLHTTP")
    Rem objHTTP.open "GET", "http://localhost:9991/", False
    objHTTP.open "POST", "http://dev.staging.my.com", False
 
    objHTTP.setRequestHeader "Content-Type", "application/x-www-form-urlencoded"
    objHTTP.send "query=test"
    Rem objHTTP.send 

    TestSearch = objHTTP.ResponseText
End Function

 
Set objHTTP = Nothing
Set domPage = Nothing