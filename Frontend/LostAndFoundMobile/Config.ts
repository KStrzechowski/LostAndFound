import React from 'react';

export const USER_ID = 'UserId';
export const USER_PHOTO_URL = 'UserPhotoUrl';
export const ACCESS_TOKEN = 'Access-Token';
export const REFRESH_TOKEN = 'Refresh-Token';
export const TOKEN_EXPIRATION_DATE = 'Token-Expiration';

export const AuthContext = React.createContext({
  signIn: async () => {},
  signOut: async () => {},
});

export const ProfileContext = React.createContext({
  updatePhotoUrl: async () => {},
  updatePhotoUrlValue: false,
});
