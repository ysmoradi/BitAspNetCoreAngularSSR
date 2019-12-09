import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpInterceptor, HttpHandler, HttpRequest, HttpResponse } from '@angular/common/http';
import { catchError, map } from "rxjs/operators";
import { throwError } from 'rxjs';
import { isPlatformServer } from '@angular/common';

@Injectable()
export class UniversalInterceptor implements HttpInterceptor {

  constructor(@Inject('BASE_URL') private baseUrl: string, @Inject(PLATFORM_ID) private platformId) { }

  intercept(req: HttpRequest<any>, next: HttpHandler) {

    // you can also use state transfer api https://medium.com/angular-in-depth/using-transferstate-api-in-an-angular-5-universal-app-130f3ada9e5b

    let serverReq: HttpRequest<any> = req;
    if (isPlatformServer(this.platformId)) {
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
