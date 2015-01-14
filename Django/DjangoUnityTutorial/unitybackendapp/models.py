from django.db import models
from django.contrib import admin
from django.contrib.auth.models import User
from django.conf import settings
import uuid
import os

# Create your models here.
class Score(models.Model):
    owner = models.ForeignKey(User)
    score = models.IntegerField()
    created = models.DateTimeField(auto_now_add=True)
    updated = models.DateTimeField(auto_now=True)
        
    def owner_name(self):
        return self.owner.username

    def __unicode__(self):
        return '%s - %d' % (self.owner.username, self.score)

class Savegame(models.Model):
    def update_filename(instance, filename):
        path = 'savegames/'
        format = '%s%s'%(instance.owner.pk, str(uuid.uuid4()))
        return os.path.join(path, format)

    owner = models.ForeignKey(User)
    name = models.CharField(max_length=100)
    file = models.FileField(upload_to=update_filename)
    created = models.DateTimeField(auto_now_add=True)
    updated = models.DateTimeField(auto_now=True)

    def __unicode__(self):
        return '%s - %s' % (self.name, self.updated)

class SavegameAdmin(admin.ModelAdmin):
    fields = ('owner', 'name', 'file')
    list_display = ['owner', 'name', 'created', 'updated']


admin.site.register(Score)
admin.site.register(Savegame, SavegameAdmin)