
Imports System.Net
Imports System.IO
Imports System.Text
Public Class InkBot
    Private mainimage As Bitmap
    Private FilesInFolder As Integer
    Private Minutes As Integer = 60
    Private auth As LinqToTwitter.SingleUserAuthorizer
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        auth = New LinqToTwitter.SingleUserAuthorizer
        auth.CredentialStore = New LinqToTwitter.SingleUserInMemoryCredentialStore
        'will need to add your twitter api keys to app.config, or hardcode them here to post to twitter.
        'will also need to add a folder path to My.Settings.ImageDirectory in app.config
        'this will be the directory where the inkblot images are stored.
        'it will store every one at the moment, so this directory will grow with time.
        'each 600x600 file is only typically between 40 and 63 kb in size.
        auth.CredentialStore.ConsumerKey = My.Settings.ConsumerKey
        auth.CredentialStore.ConsumerSecret = My.Settings.ConsumerSecret
        auth.CredentialStore.OAuthToken = My.Settings.OAuthToken
        auth.CredentialStore.OAuthTokenSecret = My.Settings.OAuthTokenSecret
        btnGenerate_Click(sender, e)
        Label1.Text = "" & Minutes & "Minutes until next tweet"
    End Sub

    'Private Function SendRequest(uri As Uri, jsonDataBytes As Byte(), contentType As String, method As String) As String
    '    Dim response As String
    '    Dim request As WebRequest

    '    request = WebRequest.Create(uri)
    '    request.ContentLength = jsonDataBytes.Length
    '    request.ContentType = contentType
    '    request.Method = method

    '    Using requestStream = request.GetRequestStream
    '        requestStream.Write(jsonDataBytes, 0, jsonDataBytes.Length)
    '        requestStream.Close()

    '        Using responseStream = request.GetResponse.GetResponseStream
    '            Using reader As New StreamReader(responseStream)
    '                response = reader.ReadToEnd()
    '            End Using
    '        End Using
    '    End Using

    '    Return response
    'End Function

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
        Dim factor = 1
        Dim BlackPen = New Pen(System.Drawing.Color.White, factor)
        mainimage = New Bitmap(600, 600)
        Dim g = Graphics.FromImage(mainimage)
        Dim x, y As Integer
        Dim ran1 As Integer
        Dim rand As Random
        rand = New Random()
        x = 0
        y = 300
        g.Clear(Color.DeepSkyBlue)
        While (x < 300 - factor And x > -300 + factor And y < 600 - factor And y > factor)
            ran1 = rand.Next(4)
            Select Case ran1
                Case 0
                    x = x + factor
                Case 1
                    x = x - factor
                Case 2
                    y = y + factor
                Case 3
                    y = y - factor
            End Select
            g.DrawRectangle(BlackPen, 300 + x, y, factor, factor)
            g.DrawRectangle(BlackPen, 300 - x, y, factor, factor)
        End While
        PictureBox1.Image = mainimage
        FilesInFolder = Directory.GetFiles(My.Settings.ImageDirectory, "*.Jpeg").Count
        Try
            mainimage.Save(My.Settings.ImageDirectory & FilesInFolder & ".jpeg", System.Drawing.Imaging.ImageFormat.Jpeg)

        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
        Console.WriteLine("Picture Saved " & FilesInFolder & ".jpeg")

        Try

            ' Task.Run(SendTweet())
            Dim action As Action = AddressOf SendTweet
            Dim task = New Task(action)
            task.Start()
            task.Wait()
            Console.WriteLine(task.Status.ToString)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Private Async Function SendTweet() As Task(Of LinqToTwitter.Status)

        ' Send Tweet!
        'Moved the commented block below to form load to avoid a race condition where it tries to send the tweet
        'before it even loads the credentials into the credentialstore
        'Dim auth = New LinqToTwitter.SingleUserAuthorizer
        'auth.CredentialStore = New LinqToTwitter.SingleUserInMemoryCredentialStore
        'auth.CredentialStore.ConsumerKey = My.Settings.ConsumerKey
        'auth.CredentialStore.ConsumerSecret = My.Settings.ConsumerSecret
        'auth.CredentialStore.OAuthToken = My.Settings.OAuthToken
        'auth.CredentialStore.OAuthTokenSecret = My.Settings.OAuthTokenSecret

        'for a good c# equivalent of this code and the basics which I built the posting part, see this, part 3 as well for the images
        'https://www.dougv.com/2015/08/posting-status-updates-to-twitter-via-linqtotwitter-part-2-plain-text-tweets/
        'TODO: remove commented out testing code/refactored code

        Dim context = New LinqToTwitter.TwitterContext(auth)
        Dim uploadedMedia As LinqToTwitter.Media
        'Try
        '    uploadedMedia = Await context.UploadMediaAsync(File.ReadAllBytes("C:\\Users\\Daryl\\Pictures\\inkblotpics\\" & FilesInFolder & ".jpeg"), "image/jpeg", Nothing, Nothing, False, Nothing)
        'Catch ex As Exception
        '    MsgBox("Error uploading picture" & vbCrLf & ex.Message)
        'Finally
        '    MessageBox.Show(uploadedMedia.MediaID)
        '    uploadedMedia.
        'End Try
        uploadedMedia = Await context.UploadMediaAsync(File.ReadAllBytes(My.Settings.ImageDirectory & FilesInFolder & ".jpeg"), "image/jpeg", Nothing, Nothing, False, Nothing)
        'Console.WriteLine("File Uploaded " & FilesInFolder & ".jpeg")
        Dim mediaIds As New List(Of ULong)
        mediaIds.Clear()

        mediaIds.Add(uploadedMedia.MediaID)
        'MessageBox.Show(uploadedMedia.Status)
        Dim s As LinqToTwitter.Status
        s = Nothing
        Try

            s = Await context.TweetAsync("What do you see?", mediaIds)
        Catch ex As Exception
            Console.WriteLine("Error Sending tweet" & vbCrLf & ex.Message)


        End Try
        Return s

    End Function

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        'right now it is set up to send a tweet on launch of the program, and then every hour. change the if statement below
        'if you want to change the interval.
        If Minutes = 0 Then
            btnGenerate_Click(sender, e)
            'set minutes here to what you want the interval to be.
            Minutes = 60
        Else
            Minutes = Minutes - 1
        End If
        Label1.Text = "" & Minutes & "Minutes until next tweet"
    End Sub
End Class


