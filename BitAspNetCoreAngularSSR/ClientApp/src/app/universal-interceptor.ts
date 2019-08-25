import { Injectable, Inject } from '@angular/core';
import { HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';

@Injectable()
export class UniversalInterceptor implements HttpInterceptor {

  constructor(@Inject('BASE_URL') private baseUrl: string) { }

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    let serverReq: HttpRequest<any> = req;
    if (typeof global != "undefined") {
      serverReq = serverReq.clone({ url: `${this.baseUrl}${serverReq.url}` });
    }
    return next.handle(serverReq);
  }
}
