export type ApiEnvelope<T> = {
  StatusCode: number;
  Notificacoes: { Chave: string; Mensagem: string }[];
  Data: T | null;
};

export type User = {
  Id: number;
  Name: string;
  Email: string;
  CreatedAt: string;
};

export type AuthResponse = {
  Token: string;
  User: User;
};

export type Customer = {
  Id: number;
  UserId: number;
  Name: string;
  Phone: string | null;
  Email: string | null;
  CreatedAt: string;
};

export type Service = {
  Id: number;
  UserId: number;
  Name: string;
  Description: string | null;
  Price: number;
  CreatedAt: string;
};

export enum QuoteStatus {
  Draft = 0,
  Sent = 1,
  Approved = 2,
  Rejected = 3,
  Expired = 4,
}

export const QuoteStatusLabel: Record<QuoteStatus, string> = {
  [QuoteStatus.Draft]: 'Rascunho',
  [QuoteStatus.Sent]: 'Enviado',
  [QuoteStatus.Approved]: 'Aprovado',
  [QuoteStatus.Rejected]: 'Rejeitado',
  [QuoteStatus.Expired]: 'Expirado',
};

export type QuoteItem = {
  Id: number;
  ServiceId: number | null;
  Description: string;
  Quantity: number;
  UnitPrice: number;
  Total: number;
};

export type Quote = {
  Id: number;
  UserId: number;
  CustomerId: number;
  CustomerName: string;
  Status: QuoteStatus;
  Total: number;
  ValidUntil: string | null;
  CreatedAt: string;
  Items: QuoteItem[];
};

export type WhatsAppLink = {
  Url: string;
  Message: string;
};
