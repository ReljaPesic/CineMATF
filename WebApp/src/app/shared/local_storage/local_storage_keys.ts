export enum LocalStorageKeys {
  AppState = 'appState',
  AccessToken = 'accessToken',
  RefreshToken = 'refreshToken',
  // Kept alongside the tokens so a refresh call still knows the username even
  // after the access token has expired (decode() rejects an expired token).
  Username = 'username',
}
