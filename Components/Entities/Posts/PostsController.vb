'
' DotNetNuke� - http://www.dotnetnuke.com
' Copyright (c) 2002-2012
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'

Imports DotNetNuke.Modules.Blog.Data
Imports DotNetNuke.Entities.Content
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Modules.Blog.Integration
Imports DotNetNuke.Modules.Blog.Entities
Imports DotNetNuke.Modules.Blog.Entities.Blogs

Namespace Entities.Posts

 Partial Public Class PostsController

  Public Shared Sub PublishPost(Post As PostInfo, publish As Boolean, publishedByUser As Integer)
   If Post.Published = publish Then Exit Sub
   Post.Published = publish
   UpdatePost(Post, publishedByUser)
   If publish Then
    Dim blog As BlogInfo = BlogsController.GetBlog(Post.BlogID, publishedByUser)
    Dim journalUrl As String = Post.PermaLink(DotNetNuke.Entities.Portals.PortalSettings.Current)
    Dim journalUserId As Integer = publishedByUser
    If Blog.PublishAsOwner Then journalUserId = Blog.OwnerUserId
    JournalController.AddBlogPostToJournal(Post, DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.TabID, journalUserId, journalUrl)
    NotificationController.RemovePostPendingNotification(blog.ModuleID, blog.BlogID, Post.ContentItemId)
   End If
  End Sub

  Public Shared Sub DeletePost(Post As PostInfo)
   DataProvider.Instance().DeletePost(Post.ContentItemId)
   Dim blog As BlogInfo = BlogsController.GetBlog(Post.BlogID, -1)
   NotificationController.RemovePostPendingNotification(blog.ModuleID, blog.BlogID, Post.ContentItemId)
  End Sub

  Public Shared Sub DeletePost(contentItemId As Integer, blogId As Integer, portalId As Integer, vocabularyId As Integer)
   DataProvider.Instance().DeletePost(contentItemId)
   'CompletePostDelete(contentItemId, blogId, PostId, portalId, vocabularyId)
  End Sub

  Public Shared Function GetPosts(moduleId As Int32, blogID As Int32, published As Integer, locale As String, endDate As Date, authorUserId As Int32, pageIndex As Int32, pageSize As Int32, orderBy As String, ByRef totalRecords As Integer, userId As Integer, userIsAdmin As Boolean) As Dictionary(Of Integer, PostInfo)

   If pageIndex < 0 Then
    pageIndex = 0
    pageSize = Integer.MaxValue
   End If

   Dim res As New Dictionary(Of Integer, PostInfo)
   Using ir As IDataReader = DataProvider.Instance().GetPosts(moduleId, blogID, userId, userIsAdmin, published, locale, endDate, authorUserId, pageIndex, pageSize, orderBy)
    res = DotNetNuke.Common.Utilities.CBO.FillDictionary(Of Integer, PostInfo)("ContentItemID", ir, False)
    If blogID = -1 Then
     Dim blogs As Dictionary(Of Integer, BlogInfo) = BlogsController.GetBlogsByModule(moduleId, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blogs(e.BlogID)
     Next
    Else
     Dim blog As BlogInfo = BlogsController.GetBlog(blogID, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blog
     Next
    End If
    ir.NextResult()
    totalRecords = DotNetNuke.Common.Globals.GetTotalRecords(ir)
   End Using
   Return res

  End Function

  Public Shared Function GetPostsByTerm(moduleId As Int32, blogID As Int32, termId As Integer, published As Integer, locale As String, endDate As Date, authorUserId As Int32, pageIndex As Int32, pageSize As Int32, orderBy As String, ByRef totalRecords As Integer, userId As Integer, userIsAdmin As Boolean) As Dictionary(Of Integer, PostInfo)

   If pageIndex < 0 Then
    pageIndex = 0
    pageSize = Integer.MaxValue
   End If

   Dim res As New Dictionary(Of Integer, PostInfo)
   Using ir As IDataReader = DataProvider.Instance().GetPostsByTerm(moduleId, blogID, userId, userIsAdmin, termId, published, locale, endDate, authorUserId, pageIndex, pageSize, orderBy)
    res = DotNetNuke.Common.Utilities.CBO.FillDictionary(Of Integer, PostInfo)("ContentItemID", ir, False)
    If blogID = -1 Then
     Dim blogs As Dictionary(Of Integer, BlogInfo) = BlogsController.GetBlogsByModule(moduleId, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blogs(e.BlogID)
     Next
    Else
     Dim blog As BlogInfo = BlogsController.GetBlog(blogID, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blog
     Next
    End If
    ir.NextResult()
    totalRecords = DotNetNuke.Common.Globals.GetTotalRecords(ir)
   End Using
   Return res

  End Function

  Public Shared Function GetPostsByBlog(moduleId As Int32, blogID As Int32, userId As Int32, pageIndex As Int32, pageSize As Int32, orderBy As String, ByRef totalRecords As Integer) As Dictionary(Of Integer, PostInfo)

   If pageIndex < 0 Then
    pageIndex = 0
    pageSize = Integer.MaxValue
   End If

   Dim res As New Dictionary(Of Integer, PostInfo)
   Using ir As IDataReader = DataProvider.Instance().GetPostsByBlog(blogID, pageIndex, pageSize, orderBy)
    res = DotNetNuke.Common.Utilities.CBO.FillDictionary(Of Integer, PostInfo)("ContentItemID", ir, False)
    If blogID = -1 Then
     Dim blogs As Dictionary(Of Integer, BlogInfo) = BlogsController.GetBlogsByModule(moduleId, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blogs(e.BlogID)
     Next
    Else
     Dim blog As BlogInfo = BlogsController.GetBlog(blogID, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blog
     Next
    End If
    ir.NextResult()
    totalRecords = DotNetNuke.Common.Globals.GetTotalRecords(ir)
   End Using
   Return res

  End Function

  Public Shared Function SearchPosts(moduleId As Int32, blogID As Int32, searchText As String, searchTitle As Boolean, searchContents As Boolean, published As Integer, locale As String, endDate As Date, authorUserId As Int32, pageIndex As Int32, pageSize As Int32, orderBy As String, ByRef totalRecords As Integer, userId As Integer, userIsAdmin As Boolean) As Dictionary(Of Integer, PostInfo)

   If pageIndex < 0 Then
    pageIndex = 0
    pageSize = Integer.MaxValue
   End If

   Dim res As New Dictionary(Of Integer, PostInfo)
   Using ir As IDataReader = DataProvider.Instance().SearchPosts(moduleId, blogID, userId, userIsAdmin, searchText, searchTitle, searchContents, published, locale, endDate, authorUserId, pageIndex, pageSize, orderBy)
    res = DotNetNuke.Common.Utilities.CBO.FillDictionary(Of Integer, PostInfo)("ContentItemID", ir, False)
    If blogID = -1 Then
     Dim blogs As Dictionary(Of Integer, BlogInfo) = BlogsController.GetBlogsByModule(moduleId, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blogs(e.BlogID)
     Next
    Else
     Dim blog As BlogInfo = BlogsController.GetBlog(blogID, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blog
     Next
    End If
    ir.NextResult()
    totalRecords = DotNetNuke.Common.Globals.GetTotalRecords(ir)
   End Using
   Return res

  End Function

  Public Shared Function SearchPostsByTerm(moduleId As Int32, blogID As Int32, termId As Integer, searchText As String, searchTitle As Boolean, searchContents As Boolean, published As Integer, locale As String, endDate As Date, authorUserId As Int32, pageIndex As Int32, pageSize As Int32, orderBy As String, ByRef totalRecords As Integer, userId As Integer, userIsAdmin As Boolean) As Dictionary(Of Integer, PostInfo)

   If pageIndex < 0 Then
    pageIndex = 0
    pageSize = Integer.MaxValue
   End If

   Dim res As New Dictionary(Of Integer, PostInfo)
   Using ir As IDataReader = DataProvider.Instance().SearchPostsByTerm(moduleId, blogID, userId, userIsAdmin, termId, searchText, searchTitle, searchContents, published, locale, endDate, authorUserId, pageIndex, pageSize, orderBy)
    res = DotNetNuke.Common.Utilities.CBO.FillDictionary(Of Integer, PostInfo)("ContentItemID", ir, False)
    If blogID = -1 Then
     Dim blogs As Dictionary(Of Integer, BlogInfo) = BlogsController.GetBlogsByModule(moduleId, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blogs(e.BlogID)
     Next
    Else
     Dim blog As BlogInfo = BlogsController.GetBlog(blogID, userId)
     For Each e As PostInfo In res.Values
      e.Blog = blog
     Next
    End If
    ir.NextResult()
    totalRecords = DotNetNuke.Common.Globals.GetTotalRecords(ir)
   End Using
   Return res

  End Function

#Region " Private Methods "
#End Region

 End Class

End Namespace