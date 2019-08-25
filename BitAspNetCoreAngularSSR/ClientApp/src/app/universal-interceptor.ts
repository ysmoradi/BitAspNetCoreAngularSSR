import { Injectable, Inject } from '@angular/core';
import { HttpInterceptor, HttpHandler, HttpRequest, HttpResponse } from '@angular/common/http';
import { catchError, map } from "rxjs/operators";
import { throwError } from 'rxjs';

@Injectable()
export class UniversalInterceptor implements HttpInterceptor {

  constructor(@Inject('BASE_URL') private baseUrl: string) { }

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    let serverReq: HttpRequest<any> = req;
    if (typeof global != "undefined") {
      serverReq = serverReq.clone({ url: `${this.baseUrl}${serverReq.url}` });
    }
    return next.handle(serverReq).pipe(
      map(res => {
        if (res instanceof HttpResponse && res.body != null && res.body.value != null) {
          res = res.clone({ body: res.body.value });
        }
        return res;
      }),
      catchError(err => {
        return throwError(err);
      })
    );
  }
}
