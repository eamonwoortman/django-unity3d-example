from django.db import models
from django.contrib import admin
from django.contrib.auth.models import User

# Create your models here.
class Score(models.Model):
    user = models.ForeignKey(User)
    score = models.IntegerField()
    created = models.DateTimeField(auto_now_add=True)
    updated = models.DateTimeField(auto_now=True)

    def __unicode__(self):
        return '%s - %d' % (self.user.username, self.score)

admin.site.register(Score)