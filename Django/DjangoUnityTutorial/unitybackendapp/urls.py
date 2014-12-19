from django.conf.urls import patterns, url, include
from api import AddScore

urlpatterns = patterns('unitybackendapp.views',
	url(r'^$', 'home_view'),
    
    #apis
    url(r'^api/addscore', AddScore.as_view())
)