from django.conf.urls import patterns, url, include
from api import AddScore, RegisterUser, DeleteUser

urlpatterns = patterns('unitybackendapp.views',
	url(r'^$', 'home_view'),
    
    #apis
    url(r'^api/addscore', AddScore.as_view()),
    url(r'^api/registeruser', RegisterUser.as_view()),
    url(r'^api/deleteuser', DeleteUser.as_view())
    
)

#for our json interface
urlpatterns += patterns('',
    url(r'^api/get-auth-token', 'rest_framework.authtoken.views.obtain_auth_token'),
)
