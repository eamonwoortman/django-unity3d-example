from django.contrib.auth.models import User
from rest_framework import serializers
from unitybackendapp.models import Score, Savegame

"""
A serializer which autosets the owner field
snippet from 'bartvandendriessche', see https://github.com/tomchristie/django-rest-framework/issues/729#issuecomment-35377747
"""
class OwnedModelSerializer(serializers.ModelSerializer):
    class Meta:
        read_only_fields = ['owner']
        exclude = ('owner',)

class ScoreSerializer(OwnedModelSerializer):
    class Meta:
        model = Score
        fields = ('id', 'score', 'owner_name', 'updated')        

        #exclude = ('owner',)

class CreateUserSerializer(serializers.ModelSerializer):
    email = serializers.EmailField(required=True)

    class Meta:
        model = User
        fields = ('email', 'username', 'password')
        extra_kwargs = {'password': {'write_only': True}}

    def create(self, validated_data):
        user = User(
            email=validated_data['email'],
            username=validated_data['username']
        )
        user.set_password(validated_data['password'])
        user.save() 
        return user

class SavegameDetailSerializer(OwnedModelSerializer):
    class Meta:
        model = Savegame
        fields = ('name', 'file')        

class SavegameListSerializer(serializers.ModelSerializer):
    class Meta:
        fields = ('id', 'name', 'updated', 'file')
        model = Savegame

