from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import authentication, permissions
from rest_framework import status
from serializers import ScoreSerializer, CreateUserSerializer
from rest_framework.generics import DestroyAPIView
from django.contrib.auth.models import User

class UnityAPIView(APIView):
    """
    This custom API replaces any other status code than 2XX with 200 
    and adds a REAL_STATUS_CODE header containing the original status code
    """
    def finalize_response(self, request, *args, **kwargs):
        response = super(UnityAPIView, self).finalize_response(request, *args, **kwargs)
        response["REAL_STATUS"] = '%s %s' % (response.status_code, response.status_text)
        response.status_code = 200
        return response

class AddScore(UnityAPIView):
    #authentication_classes = (authentication.TokenAuthentication,)
    #permission_classes = (permissions.IsAdminUser,)

    def post(self, request, format=None):
        """
        Post a new score
        """
        serializer = ScoreSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class RegisterUser(UnityAPIView):
    def post(self, request, format=None):
        serializer = CreateUserSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    
class DeleteUser(UnityAPIView):
    def post(self, request, format=None):
        try:
            user = User.objects.get(username=request.data['username'], email=request.data['email'])
            if user.check_password(request.data['password']) is False:
                raise Exception('Invalid password')
            user.delete()
        except:
            return Response('Could not find that user', status=status.HTTP_400_BAD_REQUEST)
        return Response(status=status.HTTP_204_NO_CONTENT)